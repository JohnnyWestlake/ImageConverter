using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ImageConverter.Common
{

    public partial class ImageViewModel : BindableBase
    {
        public StorageFile File { get; private set; }
        public string Size { get; private set; }

        private ImageViewModel(StorageFile file, double filesize)
        {
            File = file;
            Size = $"{file} MB";
        }
    }



    public partial class ImageViewModel
    {
        public static async Task<List<ImageViewModel>> CreateAsync(List<StorageFile> files)
        {
            List<ImageViewModel> result = new List<ImageViewModel>();
            foreach (var file in files)
            {
                var props = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
                double size = props.Size / 1024d / 1024d;

                result.Add(new ImageViewModel(file, size));
            }
            return result;
        }
    }
}
