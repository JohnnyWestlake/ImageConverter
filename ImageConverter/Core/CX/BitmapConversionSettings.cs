using Windows.Graphics.Imaging;
using Windows.Storage;

namespace ImageConverter.Core.CX;

public sealed class BitmapConversionSettings
{
    public BitmapConversionSettings() { }

    public BitmapConversionSettings(Guid encoderId, string fileExtension, IReadOnlyList<BitmapOption> options)
    {
        EncoderId = encoderId;
        FileExtension = fileExtension;
        Options = options;
        Interpolation = BitmapInterpolationMode.Fant;
    }

    public Guid EncoderId { get; set; }

    /// <summary>
    /// File extension to use when creating the converted file.
    /// </summary>
    public string FileExtension { get; set; }

    /// <summary>
    /// File name not including file extension to use when creating the converted file.
    /// If not set, uses the source file's DisplayName
    /// </summary>
    public string TargetFileName { get; set; }

    public int ScaledWidth { get; set; }

    public int ScaledHeight { get; set; }

    public bool CopyMetadata { get; set; }

    public BitmapFrameHandling FrameHandling { get; set; }

    public BitmapInterpolationMode Interpolation { get; set; }

    public IReadOnlyList<BitmapOption> Options { get; set; }

    public CreationCollisionOption CollisionOption { get; set; }
}
