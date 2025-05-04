using ImageConverter.Core;
using ImageConverter.Core.CX;

using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageConverter.Common;

public static class ImageConverterCore
{
    public static List<string> SupportedEncodeFileTypes { get; private set; }

    public static HashSet<string> SupportedDecodeFileTypes { get; private set; }

    public static Format GetFormat(BitmapCodecInformation info)
    {
        if (info.CodecId == BitmapEncoder.JpegEncoderId || info.CodecId == BitmapDecoder.JpegDecoderId)
            return Format.Jpeg;

        if (info.CodecId == BitmapEncoder.JpegXREncoderId || info.CodecId == BitmapDecoder.JpegXRDecoderId)
            return Format.JpegXR;

        if (info.CodecId == BitmapEncoder.GifEncoderId || info.CodecId == BitmapDecoder.GifDecoderId)
            return Format.Gif;

        if (info.CodecId == BitmapEncoder.PngEncoderId || info.CodecId == BitmapDecoder.PngDecoderId)
            return Format.Png;

        if (info.CodecId == BitmapEncoder.BmpEncoderId || info.CodecId == BitmapDecoder.BmpDecoderId)
            return Format.Bmp;

        if (info.CodecId == BitmapEncoder.TiffEncoderId || info.CodecId == BitmapDecoder.TiffDecoderId)
            return Format.Tiff;

       if (info.CodecId == BitmapEncoder.HeifEncoderId || info.CodecId == BitmapDecoder.HeifDecoderId)
           return Format.Heif;

       if (info.CodecId == BitmapDecoder.WebpDecoderId)
           return Format.WebP;

        if (info.CodecId == CodecSupport.RAWImageExtensionsDecoderID)
            return Format.RAW;

        if (info.CodecId == CodecSupport.DDSDecoderID || info.CodecId == CodecSupport.DDSEncoderID)
            return Format.Dds;

        return Format.Unknown;
    }

    public static string GetPrefferedFileExtension(BitmapCodecInformation info)
    {
        if (info.CodecId == BitmapEncoder.JpegEncoderId)
            return ".jpg";

        if (info.CodecId == BitmapEncoder.JpegXREncoderId)
            return ".wdp";
            if (info.CodecId == BitmapEncoder.HeifEncoderId)
                return ".heic";

        return info.FileExtensions.FirstOrDefault();
    }

    public static string GetPrefferedDisplayName(BitmapCodecInformation info)
    {
        if (info.CodecId == BitmapEncoder.JpegXREncoderId)
            return "JPEG-XR";

        if (info.CodecId == BitmapEncoder.HeifEncoderId)
            return "HEIF";

        return info.FriendlyName.Split(' ').FirstOrDefault();
    }

    public static List<ImageFormat> GetSupportedEncodingImageFormats()
    {
        CodecSupport summary = CodecSupport.CreateSummary();

        List<ImageFormat> decodeFormats = BitmapDecoder.GetDecoderInformationEnumerator()
                                                       .Select(e => new ImageFormat(e))
                                                       .ToList();

        List<ImageFormat> encodeFormats = BitmapEncoder.GetEncoderInformationEnumerator()
                                                       .Select(e => new ImageFormat(e))
                                                       .ToList();

        if (SupportedEncodeFileTypes == null)
        {
            SupportedEncodeFileTypes = encodeFormats.SelectMany(f => f.CodecInfo.FileExtensions)
                                                    .Distinct()
                                                    .ToList();

            SupportedDecodeFileTypes = decodeFormats.SelectMany(f => f.CodecInfo.FileExtensions)
                                                    .Distinct()
                                                    .Select(e => e.ToLower())
                                                    .ToHashSet();
        }

        /* Creates a list of output formats (for store listings) */
#if DEBUG
        string fo = string.Join(Environment.NewLine, SupportedDecodeFileTypes);
#endif

        return encodeFormats;
    }

    public static async Task<BitmapSize> ConvertAsync(
        List<ImageViewModel> images, 
        StorageFolder targetFolder, 
        BitmapConversionSettings settings,
        Action<int> progressCallback)
    {
        foreach (var image in images)
        {
            image.Status = image.ExtendedStatus = null;
        }

        uint success = 0;
        uint fail = 0;

        int i = 1;
        foreach (var image in images)
        {
            progressCallback(i);
            i++;
            image.Status = "Converting...";

            try
            {
                BitmapConversionResult result =
                    await BitmapEncoderFactory.EncodeAsync(image.File, targetFolder, settings).AsTask().ConfigureAwait(false);

                if (result.Success)
                {
                    image.Status = $"Converted ({result.ResultFileSize / 1024d / 1024d:0.00} MB)";
                    image.LastSuccess = true;
                    success++;
                }
                else
                {
                    image.Status = "Failed";
                    image.ExtendedStatus = result.Status;
                    image.LastSuccess = false;
                    fail++;
                }
            }
            catch
            {
                image.Status = "Failed";
                image.ExtendedStatus = "Unspecified error";
                fail++;
            }
           
        }

        return new BitmapSize { Width = success, Height = fail };
    }
}
