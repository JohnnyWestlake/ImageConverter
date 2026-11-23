using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace ImageConverter.Core.CX;

public sealed class JpegQualityOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Jpeg, Format.JpegXR, Format.Heif];

    public float ImageQuality { get; set; } = 0.9f;

    public static string Description => "Higher values indicate higher quality at the cost of larger file sizes.";

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(ImageQuality, PropertyType.Single);
        return new BitmapOption("ImageQuality", value);
    }
}

public sealed class JpegChromaSubsamplingOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Jpeg];

    public static string Description => "Specifies which chroma subsampling mode will be used for image compression in JPEG images.";

    public JpegSubsamplingMode JpegYCrCbSubsampling { get; set; } = JpegSubsamplingMode.Default;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue((byte)JpegYCrCbSubsampling, PropertyType.UInt8);
        return new BitmapOption("JpegYCrCbSubsampling", value);
    }
}

public sealed class PngFilterModeOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Png];

    public static string Description =>
        "Specifies the filter used to optimize the image prior to image compression.\n" +
        "None does not perform any filtering and is typically the fastest but consumes the most space. " +
        "Sub, Up, Average and Paeth filtering perform differently across various images. " +
        "Adaptive filtering attempts to select the most efficient of the previous filter modes for each scanline in the image. This typically performs the slowest but consumes the least space.";

    public PngFilterMode FilterOption { get; set; } = PngFilterMode.Automatic;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue((byte)FilterOption, PropertyType.UInt8);
        return new BitmapOption("FilterOption", value);
    }
}

public sealed class InterlaceOptionOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Png];

    public static string Description => "Specifies whether to encode the image data as interlaced.";

    public bool InterlaceOption { get; set; } = false;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(InterlaceOption, PropertyType.Boolean);
        return new BitmapOption("InterlaceOption", value);
    }
}

public sealed class LosslessOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.JpegXR];

    public static string Description => "If enabled, the Image Quality option is ignored.";

    public bool Lossless { get; set; } = false;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(Lossless, PropertyType.Boolean);
        return new BitmapOption("Lossless", value);
    }
}

public sealed class ProgressiveModeOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.JpegXR];

    public static string Description => "Switch between sequential or progressive encoding";

    public bool ProgressiveMode { get; set; } = true;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(ProgressiveMode, PropertyType.Boolean);
        return new BitmapOption("ProgressiveMode", value);
    }
}

public sealed class BitmapV5BGRAOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Bmp];

    public static string Description =>
        "Specifies whether to allow encoding data in the 32bpp BGRA pixel format, allowing an alpha channel to be encoded correctly. If this option is set to true, the BMP will be written with a BITMAPV5HEADER header." +
        "\n\nNote for 16-bit and 32-bit Windows BMP files, the BMP codec ignores any alpha channel, as many legacy image files contain invalid data in this extra channel. Starting with Windows 8, 32-bit Windows BMP files written using the BITMAPV5HEADER with valid alpha channel content are read as 32bpp BGRA";

    public bool EnableAlpha { get; set; } = false;

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(EnableAlpha, PropertyType.Boolean);
        return new BitmapOption("EnableV5Header32bppBGRA", value);
    }
}

public sealed class TiffCompressionQualityOption : IBitmapEncodingOption
{
    public static IReadOnlyList<Format> SupportedFormats { get; } = [Format.Tiff];

    public float CompressionQuality { get; set; } = 0.9f;

    public static string Description => "Higher values indicate a more efficient and slower compression scheme. \nCompression quality does not affect image quality, only the time taken to save the files and the resulting file size.";

    public BitmapOption GetValue()
    {
        var value = new BitmapTypedValue(CompressionQuality, PropertyType.Single);
        return new BitmapOption("CompressionQuality", value);
    }
}
