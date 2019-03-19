using ImageConverter.Core;
using ImageConverter.Core.CX;
using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;

namespace ImageConverter.Bitmap
{
    public class BitmapV5BGRAOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Bmp };

        public static string Description =>
            "Specifies whether to allow encoding data in the 32bpp BGRA pixel format, allowing an alpha channel to be encoded correctly. If this option is set to true, the BMP will be written with a BITMAPV5HEADER header." +
            "\n\nNote for 16-bit and 32-bit Windows BMP files, the BMP codec ignores any alpha channel, as many legacy image files contain invalid data in this extra channel. Starting with Windows 8, 32-bit Windows BMP files written using the BITMAPV5HEADER with valid alpha channel content are read as 32bpp BGRA";

        public bool EnableAlpha { get; set; } = false;

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(EnableAlpha, Windows.Foundation.PropertyType.Boolean);
            return new BitmapOption("EnableV5Header32bppBGRA", value);
        }
    }

    public class ProgressiveModeOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.JpegXR };

        public static string Description => "Switch between sequential or progressive encoding";

        public bool ProgressiveMode { get; set; } = true;

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(ProgressiveMode, Windows.Foundation.PropertyType.Boolean);
            return new BitmapOption(nameof(ProgressiveMode), value);
        }
    }

    public class InterlaceOptionOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Png };

        public static string Description => "Specifies whether to encode the image data as interlaced.";

        public bool InterlaceOption { get; set; }

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(InterlaceOption, Windows.Foundation.PropertyType.Boolean);
            return new BitmapOption(nameof(InterlaceOption), value);
        }
    }

    public class JpegQualityOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Jpeg, Format.JpegXR, Format.Heif };

        public static string Description => "Higher values indicate higher quality at the cost of larger file sizes.";

        public float ImageQuality { get; set; } = 0.9f;

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(ImageQuality, Windows.Foundation.PropertyType.Single );
            return new BitmapOption(nameof(ImageQuality), value);
        }
    }

    public class TiffCompressionQualityOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Tiff };

        public static string Description => "Higher values indicate a more efficient and slower compression scheme. \nCompression quality does not affect image quality, only the time taken to save the files and the resulting file size.";

        public float CompressionQuality { get; set; } = 0.9f;

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(CompressionQuality, Windows.Foundation.PropertyType.Single);
            return new BitmapOption(nameof(CompressionQuality), value);
        }
    }

    public class LosslessOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.JpegXR };

        public static string Description => "If enabled, the Image Quality option is ignored.";

        public bool Lossless { get; set; }

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue(Lossless, Windows.Foundation.PropertyType.Boolean);
            return new BitmapOption(nameof(Lossless), value);
        }
    }


    public class PngFilterModeOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Png  };

        public static string Description => 
            "Specifies the filter used to optimize the image prior to image compression.\n" +
            "None does not perform any filtering and is typically the fastest but consumes the most space. " +
            "Sub, Up, Average and Paeth filtering perform differently across various images. " +
            "Adaptive filtering attempts to select the most efficient of the previous filter modes for each scanline in the image. This typically performs the slowest but consumes the least space.";

        public PngFilterMode FilterOption { get; set; } 

        public BitmapOption GetValue()
        {
            var value = new BitmapTypedValue((byte)FilterOption, Windows.Foundation.PropertyType.UInt8);
            return new BitmapOption(nameof(FilterOption), value);
        }
    }

    public interface IBitmapEncodingOption
    {
        BitmapOption GetValue();
    }
}
