using ImageConverter.Bitmap;
using ImageConverter.Common;
using ImageConverter.Core;
using ImageConverter.Core.CX;
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
        public List<ImageFormat> ImageFormats { get; }
        private OptionsViewModel _optionsViewModel { get; } = new OptionsViewModel();

        public ObservableCollection<ImageViewModel> FileList { get; }
        public ObservableCollection<object> SelectedFiles { get; }

        public int ConvertIndex         { get => GetV(0); private set => Set(value); }
        public bool IsBusy              { get => GetV(false); private set => Set(value); }
        public bool IsProcessing        { get => GetV(false); private set => Set(value); }
        public bool IsOverwrite         { get => GetV(false); set => Set(value); }
        public bool HasSelectedItems    => SelectedFiles.Count > 0;
        public bool HasItems            => FileList.Count > 0;
        public bool HasExportFolder     => ExportFolder != null;
        public bool HasOptions          => true;

        public string StatusBarLeft     { get => Get<string>(); private set => Set(value); }
        public string StatusBarRight    { get => Get<string>(); private set => Set(value); }

        public StorageFolder ExportFolder
        {
            get => Get<StorageFolder>();
            private set => Set(value, nameof(ExportFolder), nameof(HasExportFolder));
        }

        public ImageFormat SelectedFormat
        {
            get => Get<ImageFormat>();
            set
            {
                if (Set(value))
                {
                    _optionsViewModel.SetFormat(value);
                }
            }
        }




        public MainViewModel()
        {
            FileList = new ObservableCollection<ImageViewModel>();
            SelectedFiles = new ObservableCollection<object>();

            SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
            ImageFormats = ImageConverterCore.GetSupportedEncodingImageFormats();
            SelectedFormat = ImageFormats.FirstOrDefault();
            UpdateStatusText();
        }

        private void SelectedFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSelectedItems));
            UpdateStatusText(false);
        }

        public void ResetProgress()
        {
            StatusBarRight = string.Empty;
        }

        public void UpdateStatusText(bool updateHasItems = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{FileList.Count} item");
            if (FileList.Count != 1)
                sb.Append("s");

            if (SelectedFiles.Count > 0)
                sb.Append($", {SelectedFiles.Count} selected");

            StatusBarLeft = sb.ToString();

            if (updateHasItems)
                OnPropertyChanged(nameof(HasItems));
        }


        public void OptionsClick()
        {
            OptionsDialog.Show(_optionsViewModel);
        }

        public void AddFilesClick()
        {
            _ = AddFilesAsync();
        }

        public void RemoveFilesClick()
        {
            RemoveFiles();
        }

        public void DestinationClick()
        {
            _ = ChooseExportFolderAsync();
        }

        public void ClearClick()
        {
            FileList.Clear();
            UpdateStatusText();
            ResetProgress();
            GC.Collect();
        }

        public void ConvertClick()
        {
            _ = ConvertAsync();
        }

        public void SetNewName(bool b)
        {
            IsOverwrite = !b;
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

        async Task AddFilesAsync()
        {
            var picker = new FileOpenPicker
            {
                CommitButtonText = "Select"
            };

            foreach (var type in ImageConverterCore.SupportedDecodeFileTypes)
                picker.FileTypeFilter.Add(type);

            IsBusy = true;

            var files = await picker.PickMultipleFilesAsync();

            IsProcessing = true;
            List<StorageFile> _tooAdd = new List<StorageFile>();
            foreach(var file in files)
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


            UpdateStatusText();

            if (_tooAdd.Count > 0)
                ResetProgress();

            IsProcessing = false;
            IsBusy = false;
        }

        async Task ConvertAsync()
        {
            if (FileList.Count == 0)
                return;

            IsBusy = true;
            ConvertIndex = 0;

            var options = new BitmapConversionSettings(
                SelectedFormat.CodecInfo.CodecId,
                _optionsViewModel.CurrentFileFormat,
                _optionsViewModel.GetEffectiveOptions().Select(o => o.GetValue()).ToList())
            {
                CollisionOption = IsOverwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName
            };

            int count = FileList.Count;
            var result = await ImageConverterCore.ConvertAsync(FileList.ToList(), ExportFolder, options, i =>
            {
                ConvertIndex = i - 1;
                StatusBarRight = $"Converting {i} of {count}...";
            });

            StringBuilder b = new StringBuilder();
            if (result.Width > 0 && result.Height == 0)
                StatusBarRight = $"Result: {result.Width} items converted";
            else
                StatusBarRight = $"Result: {result.Width} items converted, {result.Height} failed";

            IsBusy = false;
        }

        void RemoveFiles()
        {
            if (SelectedFiles.Count == 0)
                return;

            var files = SelectedFiles.ToList();
            SelectedFiles.Clear();

            foreach (var file in files.Cast<ImageViewModel>())
                FileList.Remove(file);

            UpdateStatusText();
            ResetProgress();
            GC.Collect();
        }
    }
}
