using ImageConverter.Bitmap;
using ImageConverter.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ImageConverter.Views
{
    public class MainViewModel : BindableBase
    {
        public ObservableCollection<ImageViewModel> FileList { get; }

        public bool IsBusy { get => GetV(false); set => Set(value); }

        public StorageFolder ExportFolder
        {
            get => Get<StorageFolder>();
            set => Set(value);
        }

        public MainViewModel()
        {
            FileList = new ObservableCollection<ImageViewModel>();
        }

        private async Task ChooseExportFolderAsync()
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ExportFolder = folder;
            }
        }

        public void AddFilesClick()
        {
            Task _ = AddFilesAsync();
        }

        public void DestinationClick()
        {
            Task _ = ChooseExportFolderAsync();
        }

        public void ConvertClick()
        {
            Task _ = ConvertAsync();
        }

        async Task AddFilesAsync()
        {
            var picker = new FileOpenPicker
            {
                CommitButtonText = "Select"
            };

            foreach (var type in Common.ImageConverter.SupportedFileTypes)
                picker.FileTypeFilter.Add(type);

            var files = await picker.PickMultipleFilesAsync();

            List<StorageFile> _tooAdd = new List<StorageFile>();
            foreach( var file in files)
            {
                if (!FileList.Any(f => f.File.Path == file.Path))
                {
                    _tooAdd.Add(file);
                }
            }

            foreach (var item in await ImageViewModel.CreateAsync(_tooAdd))
            {
                FileList.Add(item);
            }
        }

        async Task ConvertAsync()
        {
            if (FileList.Count == 0)
                return;

            IsBusy = true;

            var options = new ConversionOptions
            {
                EncoderId = BitmapEncoder.JpegXREncoderId,
                FileExtention = ".wdp"
            };

            options.EncodingOptions.Add(new JpegQualityOption { ImageQuality = 0.9f });
            await Common.ImageConverter.ConvertAsync(FileList.ToList(), ExportFolder, options);

            IsBusy = false;
        }
    }
}
