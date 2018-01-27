using ImageConverter.Core;
using ImageConverter.Core.CX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageConverter.Common
{
    public static class ImageConverterCore
    {
        public static List<string> SupportedFileTypes = new List<string>
        {
            ".jpg", ".jpeg", ".jxr", ".gif", ".tiff", ".png", ".wdp", ".hdp", ".bmp"
        };

        public static Format GetFormat(BitmapCodecInformation info)
        {
            if (info.CodecId == BitmapEncoder.JpegEncoderId)
                return Format.Jpeg;

            if (info.CodecId == BitmapEncoder.JpegXREncoderId)
                return Format.JpegXR;

            if (info.CodecId == BitmapEncoder.GifEncoderId)
                return Format.Gif;

            if (info.CodecId == BitmapEncoder.PngEncoderId)
                return Format.Png;

            if (info.CodecId == BitmapEncoder.BmpEncoderId)
                return Format.Bmp;

            if (info.CodecId == BitmapEncoder.TiffEncoderId)
                return Format.Tiff;

            if (info.FriendlyName.StartsWith("DDS"))
                return Format.Dds;

            return Format.Unknown;
        }

        public static string GetPrefferedFileExtension(BitmapCodecInformation info)
        {
            if (info.CodecId == BitmapEncoder.JpegEncoderId)
                return ".jpg";

            if (info.CodecId == BitmapEncoder.JpegXREncoderId)
                return ".wdp";

            return info.FileExtensions.FirstOrDefault();
        }

        public static string GetPrefferedDisplayName(BitmapCodecInformation info)
        {
            if (info.CodecId == BitmapEncoder.JpegXREncoderId)
                return "JPEG-XR";

            return info.FriendlyName.Split(' ').FirstOrDefault();
        }

        public static List<ImageFormat> GetSupportedEncodingImageFormats()
        {
            return BitmapEncoder.GetEncoderInformationEnumerator()
                .Select(e => new ImageFormat(e))
                .ToList();
        }

        public static async Task ConvertAsync(List<ImageViewModel> images, StorageFolder targetFolder, BitmapConversionSettings settings)
        {
            foreach (var image in images)
            {
                image.Status = image.ExtendedStatus = null;
            }

            foreach (var image in images)
            {
                image.Status = "Converting...";

                BitmapConversionResult result = 
                    await BitmapEncoderFactory.EncodeAsync(image.File, targetFolder, settings).AsTask().ConfigureAwait(false);

                if (result.Success)
                {
                    image.Status = $"Converted ({result.ResultFileSize / 1024d / 1024d:0.00} MB)";
                }
                else
                {
                    image.Status = "Failed";
                    image.ExtendedStatus = result.Status;
                }
            }
        }
    }
}
