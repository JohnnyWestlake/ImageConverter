using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageConverter.Common
{
    public static class ImageConverter
    {
        public static List<string> SupportedFileTypes = new List<string>
        {
            ".jpg", ".jpeg", ".jxr", ".gif", ".tiff", ".png", ".wdp", ".hdp", ".bmp"
        };

        public static async Task ConvertAsync(List<ImageViewModel> images, StorageFolder targetFolder, ConversionOptions options)
        {
            foreach (var image in images)
            {
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
