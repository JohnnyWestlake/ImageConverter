using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImageConverter.Core.CX;

public static class BitmapEncoderFactory
{
    private static readonly string[] s_commonMetadataProperties =
    [
        "System.Photo.DateTaken",
        "System.Photo.CameraManufacturer",
        "System.Photo.CameraModel",
        "System.Photo.FNumber",
        "System.Photo.ExposureTime",
        "System.Photo.ISOSpeed",
        "System.Photo.FocalLength",
        "System.Photo.FocalLengthInFilm",
        "System.Photo.Flash",
        "System.Photo.ExposureBias",
        "System.Photo.ExposureProgram",
        "System.Photo.MeteringMode",
        "System.Photo.WhiteBalance",
        "System.Photo.LensManufacturer",
        "System.Photo.LensModel",
        "System.GPS.LatitudeNumerator",
        "System.GPS.LatitudeDenominator",
        "System.GPS.LatitudeRef",
        "System.GPS.LongitudeNumerator",
        "System.GPS.LongitudeDenominator",
        "System.GPS.LongitudeRef",
        "System.GPS.AltitudeNumerator",
        "System.GPS.AltitudeDenominator",
        "System.GPS.AltitudeRef",
        "System.ApplicationName",
        "System.Author",
        "System.Comment",
        "System.Copyright",
        "System.Title",
        "System.Subject",
        "System.Rating"
    ];
    public static async Task<BitmapConversionResult> EncodeAsync(
        StorageFile sourceFile,
        IStorageFolder targetFolder,
        BitmapConversionSettings settings)
    {
        var result = new BitmapConversionResult();

        using IRandomAccessStream inputStream = !string.IsNullOrEmpty(sourceFile.Path)
            ? new FileStream(sourceFile.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous).AsRandomAccessStream()
            : await sourceFile.OpenAsync(FileAccessMode.Read).AsTask().ConfigureAwait(false);

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
            string defaultName = !string.IsNullOrEmpty(sourceFile.Name)
                ? Path.GetFileNameWithoutExtension(sourceFile.Name)
                : sourceFile.DisplayName;

            await EncodeFramesAsync(decoder, result, defaultName, sourceFile, settings, targetFolder).ConfigureAwait(false);
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
        StorageFile sourceFile,
        BitmapConversionSettings settings,
        IStorageFolder targetFolder)
    {
        string baseName = GetFileName(defaultName, settings);
        uint frameCount = settings.FrameHandling == BitmapFrameHandling.FirstFrameOnly
            ? 1u
            : decoder.FrameCount;

        string sourceFilePath = sourceFile.Path;
        string targetFolderPath = !string.IsNullOrEmpty(targetFolder?.Path)
            ? targetFolder.Path
            : (!string.IsNullOrEmpty(sourceFilePath) ? Path.GetDirectoryName(sourceFilePath) : null);
        bool useWin32Target = !string.IsNullOrEmpty(targetFolderPath);

        for (uint frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            string frameSuffix = (decoder.FrameCount > 1 && settings.FrameHandling == BitmapFrameHandling.ExtractAllSeperately)
                ? $" {frameIndex}"
                : string.Empty;

            if (useWin32Target)
            {
                string createdFilePath = null;
                try
                {
                    using (FileStream fileStream = CreateOutputFileStream(targetFolderPath, baseName, frameSuffix, settings.FileExtension, settings.CollisionOption, out createdFilePath))
                    using (IRandomAccessStream outputStream = fileStream.AsRandomAccessStream())
                    {
                        await EncodeInternalAsync(decoder, frameIndex, outputStream, settings).ConfigureAwait(false);
                        result.ResultFileSize += outputStream.Size;
                        result.Success = true;
                    }

                    if (settings.CopyMetadata && !string.IsNullOrEmpty(sourceFilePath) && !string.IsNullOrEmpty(createdFilePath))
                    {
                        try
                        {
                            File.SetCreationTimeUtc(createdFilePath, File.GetCreationTimeUtc(sourceFilePath));
                            File.SetLastWriteTimeUtc(createdFilePath, File.GetLastWriteTimeUtc(sourceFilePath));
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                    if (!string.IsNullOrEmpty(createdFilePath))
                    {
                        try { File.Delete(createdFilePath); } catch { }
                    }
                    throw;
                }
            }
            else
            {
                targetFolder ??= await sourceFile.GetParentAsync().AsTask().ConfigureAwait(false);
                if (targetFolder == null)
                    throw new InvalidOperationException("Target folder unavailable.");

                string fileName = $"{baseName}{frameSuffix}{settings.FileExtension}";
                StorageFile outputFile = await targetFolder.CreateFileAsync(fileName, settings.CollisionOption).AsTask().ConfigureAwait(false);
                try
                {
                    using IRandomAccessStream outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false);
                    await EncodeInternalAsync(decoder, frameIndex, outputStream, settings).ConfigureAwait(false);
                    result.ResultFileSize += outputStream.Size;
                    result.Success = true;
                }
                catch
                {
                    try { await outputFile.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask().ConfigureAwait(false); } catch { }
                    throw;
                }
            }
        }
    }

    private static FileStream CreateOutputFileStream(
        string folderPath,
        string baseName,
        string frameSuffix,
        string extension,
        CreationCollisionOption collisionOption,
        out string createdFilePath)
    {
        string basePath = Path.Combine(folderPath, $"{baseName}{frameSuffix}{extension}");

        switch (collisionOption)
        {
            case CreationCollisionOption.ReplaceExisting:
            {
                var stream = new FileStream(basePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
                createdFilePath = basePath;
                return stream;
            }

            case CreationCollisionOption.GenerateUniqueName:
                try
                {
                    var stream = new FileStream(basePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
                    createdFilePath = basePath;
                    return stream;
                }
                catch (IOException) when (File.Exists(basePath))
                {
                    for (int i = 1; ; i++)
                    {
                        string candidate = Path.Combine(folderPath, $"{baseName}{frameSuffix} ({i}){extension}");
                        try
                        {
                            var stream = new FileStream(candidate, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
                            createdFilePath = candidate;
                            return stream;
                        }
                        catch (IOException) when (File.Exists(candidate))
                        {
                        }
                    }
                }

            case CreationCollisionOption.OpenIfExists:
            {
                var stream = new FileStream(basePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
                createdFilePath = basePath;
                return stream;
            }

            case CreationCollisionOption.FailIfExists:
            default:
            {
                var stream = new FileStream(basePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
                createdFilePath = basePath;
                return stream;
            }
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

    private static Task<BitmapEncoder> CreateEncoderAsync(
        IRandomAccessStream stream,
        BitmapConversionSettings settings)
    {
        return BitmapEncoder.CreateAsync(settings.EncoderId, stream, settings.GetEncoderOptionsMap()).AsTask();
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

        bool ifd = false;
        bool exif = false;
        bool xmp = false;
        bool gps = false;

        if ((IsJpegCodec(decodeCodecId) || IsIfdBasedCodec(decodeCodecId)) &&
            (IsJpegCodec(encodeCodecId) || IsIfdBasedCodec(encodeCodecId)))
        {
            ifd = await TryCopyMetadataSetAsync(decoder, encoder, GetIFDPath(decodeCodecId), GetIFDPath(encodeCodecId)).ConfigureAwait(false);
            exif = await TryCopyMetadataSetAsync(decoder, encoder, GetExifPath(decodeCodecId), GetExifPath(encodeCodecId)).ConfigureAwait(false);
            xmp = await TryCopyMetadataSetAsync(decoder, encoder, GetXMPPath(decodeCodecId), GetXMPPath(encodeCodecId)).ConfigureAwait(false);
            gps = await TryCopyMetadataSetAsync(decoder, encoder, GetGPSPath(decodeCodecId), GetGPSPath(encodeCodecId)).ConfigureAwait(false);
        }

        bool fallback = false;
        if (!ifd || !exif || !gps)
        {
            fallback = await TryCopyFallbackPropertiesAsync(decoder, encoder).ConfigureAwait(false);
        }

        try
        {
            var orientationMap = new Dictionary<string, BitmapTypedValue>
            {
                ["System.Photo.Orientation"] = new BitmapTypedValue((ushort)1, Windows.Foundation.PropertyType.UInt16)
            };
            await encoder.BitmapProperties.SetPropertiesAsync(orientationMap).AsTask().ConfigureAwait(false);
        }
        catch
        {
        }

        return ifd || exif || xmp || gps || fallback;
    }

    private static async Task<bool> TryCopyFallbackPropertiesAsync(
        BitmapDecoder decoder,
        BitmapEncoder encoder)
    {
        BitmapPropertySet props;
        try
        {
            props = await decoder.BitmapProperties.GetPropertiesAsync(s_commonMetadataProperties).AsTask().ConfigureAwait(false);
        }
        catch
        {
            props = new BitmapPropertySet();
            foreach (string key in s_commonMetadataProperties)
            {
                try
                {
                    BitmapPropertySet single = await decoder.BitmapProperties.GetPropertiesAsync(new[] { key }).AsTask().ConfigureAwait(false);
                    if (single.TryGetValue(key, out BitmapTypedValue val))
                    {
                        props[key] = val;
                    }
                }
                catch
                {
                }
            }
        }

        if (props == null || props.Count == 0)
            return false;

        bool anyCopied = false;
        foreach (var kvp in props)
        {
            try
            {
                var singleMap = new Dictionary<string, BitmapTypedValue>
                {
                    [kvp.Key] = kvp.Value
                };
                await encoder.BitmapProperties.SetPropertiesAsync(singleMap).AsTask().ConfigureAwait(false);
                anyCopied = true;
            }
            catch
            {
            }
        }

        return anyCopied;
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
        id == BitmapDecoder.WebpDecoderId || id == CodecSupport.RAWImageExtensionsDecoderID;

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
