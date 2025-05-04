﻿using Windows.Storage;

namespace ImageConverter.Common;

public partial class ImageViewModel : BindableBase
{
    public StorageFile File         { get; private set; }
    public string Size              { get; private set; }
    public string Status            { get => Get<String>(); set => Set(value); }
    public string ExtendedStatus    { get => Get<String>(); set => Set(value); }
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
        List<ImageViewModel> result = [];
        foreach (var file in files)
        {
            var props = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);
            double size = props.Size / 1024d / 1024d;

            result.Add(new ImageViewModel(file, size));
        }
        return result;
    }
}
