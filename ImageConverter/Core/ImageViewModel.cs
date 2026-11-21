using Windows.Storage;

namespace ImageConverter.Common;

[WinRT.GeneratedBindableCustomProperty]
public partial class ImageViewModel : BindableBase
{
    private string _status;
    private string _extendedStatus;

    public StorageFile File         { get; private set; }
    public string Size              { get; private set; }
    public string Status            { get => _status; set => Set(ref _status, value); }
    public string ExtendedStatus    { get => _extendedStatus; set => Set(ref _extendedStatus, value); }
    public bool LastSuccess         { get; set; } = true;

    private ImageViewModel(StorageFile file, double filesize)
    {
        File = file;
        Size = $"{filesize:0.00} MB";
    }
}

public partial class ImageViewModel
{
    public static async Task<List<ImageViewModel>> CreateAsync(List<StorageFile> files)
    {
        if (files == null || files.Count == 0)
            return [];

        var array = new ImageViewModel[files.Count];
        await Parallel.ForAsync(0, files.Count, async (i, _) =>
        {
            StorageFile file = files[i];
            double size;
            if (!string.IsNullOrEmpty(file.Path))
            {
                size = new FileInfo(file.Path).Length / 1024d / 1024d;
            }
            else
            {
                var props = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
                size = props.Size / 1024d / 1024d;
            }

            array[i] = new ImageViewModel(file, size);
        }).ConfigureAwait(false);

        return [.. array];
    }
}
