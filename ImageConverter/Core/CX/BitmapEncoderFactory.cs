using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageConverter.Core.CX;

public static class BitmapEncoderFactory
{
    public static async Task<BitmapConversionResult> EncodeAsync(
        StorageFile sourceFile,
        IStorageFolder targetFolder,
        BitmapConversionSettings settings)
    {
        var result = new BitmapConversionResult();

        using IRandomAccessStream inputStream = await sourceFile.OpenAsync(FileAccessMode.Read).AsTask().ConfigureAwait(false);

        BitmapDecoder decoder;
        try
        {
            decoder = await BitmapDecoder.CreateAsync(inputStream).AsTask().ConfigureAwait(false);
        }
        catch
        {
            result.Status = "Could not decode file";
            return result;
        }

        try
        {
            await EncodeFramesAsync(decoder, result, sourceFile.DisplayName, settings, targetFolder).ConfigureAwait(false);
        }
        catch
        {
            result.Status = "Could not write to output file";
            result.Success = false;
        }

        return result;
    }

    public static async Task<BitmapConversionResult> EncodeAsync(
        IRandomAccessStream inputStream,
        IRandomAccessStream outputStream,
        uint inputFrameIndex,
        bool keepInputStreamAlive,
        BitmapConversionSettings settings)
    {
        var result = new BitmapConversionResult();

        BitmapDecoder decoder;
        try
        {
            decoder = await BitmapDecoder.CreateAsync(inputStream).AsTask().ConfigureAwait(false);
        }
        catch
        {
            result.Status = "Could not decode input stream";
            return result;
        }

        try
        {
            try
            {
                await EncodeInternalAsync(decoder, inputFrameIndex, outputStream, settings).ConfigureAwait(false);
                result.Success = true;
            }
            catch
            {
                result.Status = "Could not write to output stream";
            }
        }
        finally
        {
            if (!keepInputStreamAlive)
                inputStream.Dispose();
        }

        return result;
    }

    private static string GetFileName(string defaultName, BitmapConversionSettings settings)
    {
        if (string.IsNullOrEmpty(settings.TargetFileName))
            return defaultName;

        return settings.TargetFileName;
    }

    private static async Task EncodeFramesAsync(
        BitmapDecoder decoder,
        BitmapConversionResult result,
        string defaultName,
        BitmapConversionSettings settings,
        IStorageFolder targetFolder)
    {
        string baseName = GetFileName(defaultName, settings);
        uint frameCount = settings.FrameHandling == BitmapFrameHandling.FirstFrameOnly
            ? 1u
            : decoder.FrameCount;

        for (uint frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            string fileName = (decoder.FrameCount > 1 && settings.FrameHandling == BitmapFrameHandling.ExtractAllSeperately)
                ? $"{baseName} {frameIndex}{settings.FileExtension}"
                : $"{baseName}{settings.FileExtension}";

            StorageFile outputFile = await targetFolder.CreateFileAsync(fileName, settings.CollisionOption).AsTask().ConfigureAwait(false);
            using (IRandomAccessStream outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
            {
                await EncodeInternalAsync(decoder, frameIndex, outputStream, settings).ConfigureAwait(false);
            }

            var properties = await outputFile.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
            result.ResultFileSize += properties.Size;
            result.Success = true;
        }
    }

    private static async Task EncodeInternalAsync(
        BitmapDecoder decoder,
        uint frameIndex,
        IRandomAccessStream outputStream,
        BitmapConversionSettings settings)
    {
        outputStream.Size = 0;

        IBitmapFrame frame = await GetFrameAsync(decoder, frameIndex).ConfigureAwait(false);
        BitmapEncoder encoder = await CreateEncoderAsync(outputStream, settings).ConfigureAwait(false);
        PixelDataProvider pixelData = await frame.GetPixelDataAsync().AsTask().ConfigureAwait(false);

        encoder.SetPixelData(
            frame.BitmapPixelFormat,
            BitmapAlphaMode.Premultiplied,
            frame.OrientedPixelWidth,
            frame.OrientedPixelHeight,
            frame.DpiX,
            frame.DpiY,
            pixelData.DetachPixelData());

        bool needsTransform =
            (settings.ScaledHeight > 0 && frame.OrientedPixelHeight > (uint)settings.ScaledHeight)
            || (settings.ScaledWidth > 0 && frame.OrientedPixelWidth > (uint)settings.ScaledWidth);

        if (needsTransform)
        {
            double destWidth = settings.ScaledWidth > 0
                ? Math.Min((double)settings.ScaledWidth, decoder.OrientedPixelWidth)
                : frame.OrientedPixelWidth;
            double destHeight = settings.ScaledHeight > 0
                ? Math.Min((double)settings.ScaledHeight, decoder.OrientedPixelHeight)
                : frame.OrientedPixelHeight;

            double scale = Math.Min(destWidth / decoder.OrientedPixelWidth, destHeight / decoder.OrientedPixelHeight);

            encoder.BitmapTransform.ScaledHeight = (uint)(frame.OrientedPixelHeight * scale);
            encoder.BitmapTransform.ScaledWidth = (uint)(frame.OrientedPixelWidth * scale);
            encoder.BitmapTransform.InterpolationMode = settings.Interpolation;
        }

        await HandleMetadataAsync(decoder, encoder, settings.CopyMetadata).ConfigureAwait(false);
        await encoder.FlushAsync().AsTask().ConfigureAwait(false);
    }

    private static async Task<BitmapEncoder> CreateEncoderAsync(
        IRandomAccessStream stream,
        BitmapConversionSettings settings)
    {
        var map = new Dictionary<string, BitmapTypedValue>();
        if (settings.Options != null)
        {
            foreach (BitmapOption option in settings.Options)
            {
                map[option.Name] = option.Value;
            }
        }

        return await BitmapEncoder.CreateAsync(settings.EncoderId, stream, map).AsTask().ConfigureAwait(false);
    }

    private static async Task<bool> HandleMetadataAsync(
        BitmapDecoder decoder,
        BitmapEncoder encoder,
        bool copy)
    {
        if (!copy)
            return false;

        Guid decodeCodecId = decoder.DecoderInformation.CodecId;
        Guid encodeCodecId = encoder.EncoderInformation.CodecId;

        bool ifd = await TryCopyMetadataSetAsync(decoder, encoder, GetIFDPath(decodeCodecId), GetIFDPath(encodeCodecId)).ConfigureAwait(false);
        bool exif = await TryCopyMetadataSetAsync(decoder, encoder, GetExifPath(decodeCodecId), GetExifPath(encodeCodecId)).ConfigureAwait(false);
        bool xmp = await TryCopyMetadataSetAsync(decoder, encoder, GetXMPPath(decodeCodecId), GetXMPPath(encodeCodecId)).ConfigureAwait(false);
        bool gps = await TryCopyMetadataSetAsync(decoder, encoder, GetGPSPath(decodeCodecId), GetGPSPath(encodeCodecId)).ConfigureAwait(false);

        return ifd || exif || xmp || gps;
    }

    private static async Task<bool> TryCopyMetadataSetAsync(
        BitmapDecoder decoder,
        BitmapEncoder encoder,
        string decodeFramePath,
        string encodeFramePath)
    {
        try
        {
            if (decodeFramePath == null || encodeFramePath == null)
                return false;

            BitmapPropertySet props = await decoder.BitmapProperties.GetPropertiesAsync([decodeFramePath]).AsTask().ConfigureAwait(false);
            if (props.Count == 0 || !props.TryGetValue(decodeFramePath, out BitmapTypedValue value))
                return false;

            var map = new Dictionary<string, BitmapTypedValue>
            {
                [encodeFramePath] = value
            };

            await encoder.BitmapProperties.SetPropertiesAsync(map).AsTask().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<IBitmapFrame> GetFrameAsync(BitmapDecoder decoder, uint frameIndex)
    {
        if (frameIndex == 0)
            return decoder;

        return await decoder.GetFrameAsync(frameIndex).AsTask().ConfigureAwait(false);
    }

    private static bool IsJpegCodec(Guid id) =>
        id == BitmapDecoder.JpegDecoderId || id == BitmapEncoder.JpegEncoderId;

    private static bool IsIfdBasedCodec(Guid id) =>
        id == BitmapDecoder.TiffDecoderId || id == BitmapDecoder.JpegXRDecoderId ||
        id == BitmapEncoder.TiffEncoderId || id == BitmapEncoder.JpegXREncoderId ||
        id == BitmapEncoder.HeifEncoderId || id == BitmapDecoder.HeifDecoderId ||
        id == BitmapDecoder.WebpDecoderId;

    private static string GetExifPath(Guid id) =>
        IsJpegCodec(id) ? "/app1/ifd/exif" :
        IsIfdBasedCodec(id) ? "/ifd/exif" : null;

    private static string GetXMPPath(Guid id) =>
        IsJpegCodec(id) ? "/xmp" :
        IsIfdBasedCodec(id) ? "/ifd/xmp" : null;

    private static string GetGPSPath(Guid id) =>
        IsJpegCodec(id) ? "/app1/ifd/gps" :
        IsIfdBasedCodec(id) ? "/ifd/gps" : null;

    private static string GetIFDPath(Guid id) =>
        IsJpegCodec(id) ? "/app1/ifd" :
        IsIfdBasedCodec(id) ? "/ifd" : null;
}
