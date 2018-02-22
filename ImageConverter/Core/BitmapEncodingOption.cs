using ImageConverter.Core;
using ImageConverter.Core.CX;
using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;

namespace ImageConverter.Bitmap
{
    public class JpegQualityOption : IBitmapEncodingOption
    {
        public static List<Format> SupportedFormats { get; } = new List<Format> { Format.Jpeg, Format.JpegXR };

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
