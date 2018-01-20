using ImageConverter.Core;
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

        public static async Task ConvertAsync(List<ImageViewModel> images, StorageFolder targetFolder, ConversionOptions options)
        {
            foreach (var image in images)
            {
                image.Status = image.ExtendedStatus = null;
            }

            foreach (var image in images)
            {
                image.Status = "Converting...";
                using (var stream = await image.File.OpenAsync(FileAccessMode.Read).AsTask().ConfigureAwait(false))
                {
                    BitmapDecoder decoder;
                    try
                    {
                        decoder = await BitmapDecoder.CreateAsync(stream).AsTask().ConfigureAwait(false);
                    }
                    catch
                    {
                        image.Status = "Failed";
                        image.ExtendedStatus = "Could not read file";
                        continue;
                    }

                    try
                    {
                        var outputFile = await targetFolder.CreateFileAsync($"{image.File.DisplayName}{options.FileExtention}", options.CollisionOption).AsTask().ConfigureAwait(false);
                        using (var outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
                        {
                            outputStream.Size = 0;

                            var encoder = await BitmapEncoder.CreateAsync(options.EncoderId, outputStream, options.EncodingOptions.Select(o => o.GetValue())).AsTask().ConfigureAwait(false);
                            encoder.SetPixelData(
                                decoder.BitmapPixelFormat,
                                decoder.BitmapAlphaMode,
                                decoder.OrientedPixelWidth,
                                decoder.OrientedPixelHeight,
                                decoder.DpiX,
                                decoder.DpiY,
                                (await decoder.GetPixelDataAsync().AsTask().ConfigureAwait(false)).DetachPixelData());

                            await encoder.FlushAsync().AsTask().ConfigureAwait(false);
                        }
                        var props = await outputFile.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
                        image.Status = $"Converted ({props.Size / 1024d / 1024d:0.00} MB)";
                    }
                    catch
                    {
                        image.Status = "Failed";
                        image.ExtendedStatus = "Could not write to output file";
                    }
                }
            }
        }
    }
}
