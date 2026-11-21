using ImageConverter.Common;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace ImageConverter.Views;

[WinRT.GeneratedBindableCustomProperty]
public partial class MainViewModel : BindableBase
{
    public List<ImageFormat> ImageFormats { get; }
    private OptionsViewModel _optionsViewModel { get; } = new OptionsViewModel();
    private TransformViewModel _transformViewModel { get; } = new TransformViewModel();

    public ObservableCollection<ImageViewModel> FileList { get; }
    public ObservableCollection<object> SelectedFiles { get; }

    public int ConvertIndex         { get => GetV(0); private set => Set(value); }
    public bool IsBusy              { get => GetV(false); private set => Set(value); }
    public bool IsProcessing        { get => GetV(false); private set => Set(value); }
    public bool IsOverwrite         { get => GetV(false); set => Set(value); }
    public bool CopyMetadata        { get => GetV(true); set => Set(value); }
    public bool MultiframeSupport   { get => GetV(true); set => Set(value); }
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
        set { if (Set(value)) { _optionsViewModel.SetFormat(value); } }
    }

    public CodecSupport Support { get; }


    public MainViewModel()
    {
        FileList = new ObservableCollection<ImageViewModel>();
        SelectedFiles = new ObservableCollection<object>();

        SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
        ImageFormats = ImageConverterCore.GetSupportedEncodingImageFormats();
        SelectedFormat = ImageFormats.FirstOrDefault(f => f.CodecInfo.CodecId == BitmapEncoder.JpegEncoderId);
        UpdateStatusText();

        Support = CodecSupport.CreateSummary();
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

    public void FormatsClick()
    {
        FormatsDialog.Show(Support);
    }

    public void OptionsClick()
    {
        OptionsDialog.Show(_optionsViewModel);
    }

    public void TransformClick()
    {
        TransformDialog.Show(_transformViewModel);
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

    public void OpenExportFolderClick()
    {
        _ = OpenExportFolderAsync();
    }

    public void ClearClick()
    {
        FileList.Clear();
        UpdateStatusText();
        ResetProgress();
    }

    public void ConvertClick()
    {
        _ = ConvertAsync();
    }

    public void SetNewName(bool b)
    {
        IsOverwrite = !b;
    }

    internal void ProcessDroppedFiles(IReadOnlyList<IStorageItem> items)
    {
        _ = ProcessAddFilesAsync(items.OfType<StorageFile>().ToList());
    }






    async Task ChooseExportFolderAsync()
    {
        FolderPicker picker = new FolderPicker();
        if (App.WindowHandle != IntPtr.Zero)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
        }
        picker.FileTypeFilter.Add("*");
        var folder = await picker.PickSingleFolderAsync();
        ProcessExportFolder(folder);
    }

    internal void ProcessExportFolder(StorageFolder folder)
    {
        if (folder == null ||string.IsNullOrWhiteSpace(folder.Path))
            return;

        ExportFolder = folder;
    }

    Task OpenExportFolderAsync()
    {
        if (ExportFolder == null)
            return Task.CompletedTask;

        return Launcher.LaunchFolderAsync(ExportFolder).AsTask();
    }


    async Task AddFilesAsync()
    {
        var picker = new FileOpenPicker
        {
            CommitButtonText = "Select"
        };
        if (App.WindowHandle != IntPtr.Zero)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.WindowHandle);
        }

        foreach (var type in ImageConverterCore.SupportedDecodeFileTypes)
            picker.FileTypeFilter.Add(type);

        IsBusy = true;

        var files = await picker.PickMultipleFilesAsync();
        await ProcessAddFilesAsync(files);
    }

    async Task ProcessAddFilesAsync(IEnumerable<StorageFile> files)
    {
        if (files == null)
        {
            IsProcessing = false;
            IsBusy = false;
            return;
        }

        IsBusy = true;
        IsProcessing = true;

        int onlineFiles = 0;

        List<StorageFile> heifImageSequence = new List<StorageFile>();

        var existingPaths = new HashSet<string>(
            FileList.Select(f => f.File.Path).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        List<StorageFile> _tooAdd = new List<StorageFile>();
        foreach (var file in files)
        {
            string fileType = file.FileType.ToLowerInvariant();
            if (!ImageConverterCore.SupportedDecodeFileTypes.Contains(fileType))
                continue;

            if (file.Attributes.HasFlag(Windows.Storage.FileAttributes.LocallyIncomplete))
            {
                onlineFiles++;
                continue;
            }

            // We don't allow HEIC image sequences right now.
            if (fileType is ".heic" or ".heif")
            {
                if (await IsHeifImageSequenceAsync(file))
                {
                    heifImageSequence.Add(file);
                    continue;
                }
            }

            if (string.IsNullOrEmpty(file.Path) || existingPaths.Add(file.Path))
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

        if (onlineFiles > 0 || heifImageSequence.Count > 0)
        {
            string title = "Some files have been skipped";
            StringBuilder sb = new StringBuilder();

            if (heifImageSequence.Count > 0)
            {
                sb.Append("Some of your HEIF files contain images sequences or derived images, which are not supported by Windows. As a result, the following files have not been added:");
                sb.AppendLine();
                sb.AppendLine();
                foreach (var file in heifImageSequence)
                {
                    sb.AppendFormat(" · {0}", file.Name);
                    sb.AppendLine();
                }

            }
            if (onlineFiles > 0)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }

                sb.Append(
                    "To add files from online sources such as OneDrive, please make sure they are available locally on your device first.");
            }

            MessageBox.Show(sb.ToString(), title);
        }
    }

    private static async Task<bool> IsHeifImageSequenceAsync(StorageFile file)
    {
        using Stream s = !string.IsNullOrEmpty(file.Path)
            ? new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, FileOptions.SequentialScan | FileOptions.Asynchronous)
            : await file.OpenStreamForReadAsync();

        if (s.Length <= 2048)
            return false;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(2048);
        try
        {
            int totalRead = 0;
            while (totalRead < 2048)
            {
                int read = await s.ReadAsync(buffer.AsMemory(totalRead, 2048 - totalRead)).ConfigureAwait(false);
                if (read == 0)
                    break;
                totalRead += read;
            }

            char[] chars = ArrayPool<char>.Shared.Rent(totalRead);
            try
            {
                int charCount = 0;
                for (int i = 0; i < totalRead; i++)
                {
                    char c = (char)buffer[i];
                    if (char.IsAsciiLetter(c))
                    {
                        chars[charCount++] = c;
                    }
                }

                ReadOnlySpan<char> span = chars.AsSpan(0, charCount);
                return span.Contains("ImageSequencePictureHandler", StringComparison.Ordinal)
                    || span.Contains("Derivedimage", StringComparison.Ordinal);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(chars);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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
            CollisionOption = IsOverwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName,
            CopyMetadata = CopyMetadata,
            FrameHandling = MultiframeSupport ? BitmapFrameHandling.ExtractAllSeperately : BitmapFrameHandling.FirstFrameOnly
        };

        _transformViewModel.ApplyTo(options);

        int count = FileList.Count;
        var result = await ImageConverterCore.ConvertAsync(FileList.ToList(), ExportFolder, options, completed =>
        {
            ConvertIndex = completed - 1;
            StatusBarRight = $"Converting {completed} of {count}...";
        });

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

        if (files.Count == FileList.Count)
        {
            FileList.Clear();
        }
        else
        {
            foreach (var file in files.Cast<ImageViewModel>())
                FileList.Remove(file);
        }

        UpdateStatusText();
        ResetProgress();
    }
}
