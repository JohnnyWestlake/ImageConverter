using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageConverter.Common
{
    public static class ImageLoader
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
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    var outputFile = await targetFolder.CreateFileAsync($"{image.File.DisplayName}.{options.FileExtention}", options.CollisionOption).AsTask().ConfigureAwait(false);
                    using (var outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
                    {
                        outputStream.Size = 0;
                        var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
                        await encoder.FlushAsync();
                    }
                }
            }
        }
    }
}
