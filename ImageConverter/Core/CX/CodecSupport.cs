using Windows.Graphics.Imaging;

namespace ImageConverter.Core.CX;

/// <summary>
/// Retrieves information related to installed Microsoft Store codec
/// extension packages.
/// </summary>
[WinRT.GeneratedBindableCustomProperty]
public sealed partial class CodecSupport
{
    public bool HasHEIFImageExtensions { get; set; }

    public bool HasWebPImageExtensions { get; set; }

    public bool HasRAWImageExtensions { get; set; }

    public bool HasAV1VideoExtensions { get; set; }

    public bool IsMissingExtensions { get; set; }

    /// <summary>
    /// Codec ID for Microsoft RAW Image Extension package.
    /// </summary>
    public static Guid RAWImageExtensionsDecoderID { get; } = new Guid(0x41945702, 0x8302, 0x44a6, 0x94, 0x45, 0xac, 0x98, 0xe8, 0xaf, 0xa0, 0x86);

    public static Guid DDSDecoderID { get; } = new Guid(0x9053699f, 0xa341, 0x429d, 0x9e, 0x90, 0xee, 0x43, 0x7c, 0xf8, 0x0c, 0x73);

    public static Guid DDSEncoderID { get; } = new Guid(0xa61dde94, 0x66ce, 0x4ac1, 0x88, 0x1b, 0x71, 0x68, 0x05, 0x88, 0x89, 0x5e);

    public static Uri HEIFImageExtensionsStoreLink { get; } = new Uri("ms-windows-store://pdp/?ProductId=9pmmsr1cgpwg");

    public static Uri WebPImageExtensionsStoreLink { get; } = new Uri("ms-windows-store://pdp/?ProductId=9pg2dk419drg");

    public static Uri RAWImageExtensionsStoreLink { get; } = new Uri("ms-windows-store://pdp/?ProductId=9nctdw2w1bh8");

    public static Uri AV1VideoExtensionsStoreLink { get; } = new Uri("ms-windows-store://pdp/?ProductId=9mvzqvxjbq9v");

    public static Uri HEIFImageExtensionsWebLink { get; } = new Uri("https://www.microsoft.com/store/apps/9pmmsr1cgpwg");

    public static Uri WebPImageExtensionsWebLink { get; } = new Uri("https://www.microsoft.com/store/apps/9pg2dk419drg");

    public static Uri RAWImageExtensionsWebLink { get; } = new Uri("https://www.microsoft.com/store/apps/9nctdw2w1bh8");

    public static Uri AV1VideoExtensionsWebLink { get; } = new Uri("https://www.microsoft.com/store/apps/9mvzqvxjbq9v");

    public static CodecSupport CreateSummary()
    {
        var summary = new CodecSupport();

        IReadOnlyList<BitmapCodecInformation> items = BitmapDecoder.GetDecoderInformationEnumerator();

        foreach (var info in items)
        {
            if (info.CodecId == BitmapDecoder.HeifDecoderId)
                summary.HasHEIFImageExtensions = true;
            else if (info.CodecId == BitmapDecoder.WebpDecoderId)
                summary.HasWebPImageExtensions = true;
            else if (info.CodecId == RAWImageExtensionsDecoderID)
                summary.HasRAWImageExtensions = true;

            if (info.FileExtensions.Any(e => string.Equals(e, ".avif", StringComparison.OrdinalIgnoreCase)))
                summary.HasAV1VideoExtensions = true;
        }

        if (!summary.HasHEIFImageExtensions || !summary.HasWebPImageExtensions || !summary.HasRAWImageExtensions || !summary.HasAV1VideoExtensions)
            summary.IsMissingExtensions = true;

        return summary;
    }

    internal CodecSupport() { }
}
