using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace ImageConverter.Bitmap
{
    public class JpegQualityOption : IBitmapEncodingOption
    {
        public static List<Guid> SupportedEncoders { get; } = new List<Guid> { BitmapEncoder.JpegEncoderId, BitmapEncoder.JpegXREncoderId };

        public float ImageQuality { get; set; }

        public KeyValuePair<string, BitmapTypedValue> GetValue()
        {
            var value = new BitmapTypedValue(ImageQuality, Windows.Foundation.PropertyType.Single );
            return new KeyValuePair<string, BitmapTypedValue>(nameof(ImageQuality), value);
        }
    }

    public class LosslessOption : IBitmapEncodingOption
    {
        public static List<Guid> SupportedEncoders { get; } = new List<Guid> { BitmapEncoder.JpegXREncoderId };

        public bool Lossless { get; set; }

        public KeyValuePair<string, BitmapTypedValue> GetValue()
        {
            var value = new BitmapTypedValue(Lossless, Windows.Foundation.PropertyType.Boolean);
            return new KeyValuePair<string, BitmapTypedValue>(nameof(Lossless), value);
        }
    }

    public class PngFilterModeOption : IBitmapEncodingOption
    {
        public static List<Guid> SupportedEncoders { get; } = new List<Guid> { BitmapEncoder.PngEncoderId };

        public PngFilterMode FilterMode { get; set; }

        public KeyValuePair<string, BitmapTypedValue> GetValue()
        {
            var value = new BitmapTypedValue(FilterMode, Windows.Foundation.PropertyType.UInt32);
            return new KeyValuePair<string, BitmapTypedValue>(nameof(FilterMode), value);
        }
    }

    public interface IBitmapEncodingOption
    {
        KeyValuePair<string, BitmapTypedValue> GetValue();
    }
}
