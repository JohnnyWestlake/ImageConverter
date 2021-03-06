using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageConverter.Core;
using ImageConverter.Core.CX;
using Windows.Graphics.Imaging;

namespace ImageConverter.Views
{
    public class OptionsViewModel : BindableBase
    {
        private ImageFormat _format;

        public const string HeifWarning = "The Windows 10 HEIF Encoder created by Microsoft is experimental and prone to crashing on smaller images. \n\nSource Images less than 0.02 MB in size may cause the application to crash with no warning when attempting to convert to HEIF.";

        public bool IsJpeg  => _format.Format == Format.Jpeg;
        public bool IsJxr   => _format.Format == Format.JpegXR;
        public bool IsPng   => _format.Format == Format.Png;
        public bool IsTiff  => _format.Format == Format.Tiff;
        public bool IsHeif  => _format.Format == Format.Heif;

        public bool IsQualityEnabled        => !IsLossless;
        public bool SupportsBitmapAlpha     => BitmapV5BGRAOption.SupportedFormats.Contains(_format.Format);
        public bool SupportsQuality         => JpegQualityOption.SupportedFormats.Contains(_format.Format);
        public bool SupportsLossless        => LosslessOption.SupportedFormats.Contains(_format.Format);
        public bool SupportsProgressive     => false; // Doesn't work? ProgressiveModeOption.SupportedFormats.Contains(_format.Format);
        public bool SupportsInterlace       => InterlaceOptionOption.SupportedFormats.Contains(_format.Format);

        public PngFilterMode[] PngOptions { get; } 
            = Enum.GetValues(typeof(PngFilterMode)).Cast<PngFilterMode>().ToArray();

        public JpegSubsamplingMode[] ChromaOptions { get; }
            = Enum.GetValues(typeof(JpegSubsamplingMode)).Cast<JpegSubsamplingMode>().ToArray();

        // Bindable Properties

        public List<String> FileFormats         { get; set; }
        public string CurrentFileFormat         { get => Get<String>(); set => Set(value); }
        public double JpegQuality               { get => GetV(0.9d); set => Set(value); }
        public double TiffCompressionQuality    { get => GetV(0.9d); set => Set(value); }
        public bool ProgressiveMode             { get => GetV(false); set => Set(value); }
        public bool Interlace                   { get => GetV(false); set => Set(value); }
        public bool BitmapAlpha                 { get => GetV(false); set => Set(value); }
        public PngFilterMode PngFilter          { get => GetV(PngFilterMode.Automatic); set => Set(value); }
        public JpegSubsamplingMode Chroma       { get => GetV(JpegSubsamplingMode.Default); set => Set(value); }

        public bool IsLossless
        {
            get => GetV(false);
            set => Set(value, nameof(IsLossless), nameof(IsQualityEnabled));
        }


        public OptionsViewModel() { }

        public void SetFormat(ImageFormat format)
        {
            this._format = format;

            this.FileFormats = format.CodecInfo.FileExtensions.ToList();
            this.CurrentFileFormat = format.DefaultFileExtension;

            if (!SupportsLossless)
                IsLossless = false;
        }

        public List<IBitmapEncodingOption> GetEffectiveOptions()
        {
            List<IBitmapEncodingOption> options = new List<IBitmapEncodingOption>();

            if (SupportsQuality && !IsLossless)
                options.Add(new JpegQualityOption { ImageQuality = (float)JpegQuality });

            if (SupportsLossless && IsLossless)
                options.Add(new LosslessOption { Lossless = IsLossless });

            if (SupportsProgressive)
                options.Add(new ProgressiveModeOption { ProgressiveMode = ProgressiveMode });

            if (SupportsInterlace)
                options.Add(new InterlaceOptionOption { InterlaceOption = Interlace });

            if (SupportsBitmapAlpha)
                options.Add(new BitmapV5BGRAOption { EnableAlpha = BitmapAlpha });

            if (IsJpeg)
                options.Add(new JpegChromaSubsamplingOption { JpegYCrCbSubsampling = Chroma });

            if (IsTiff)
                options.Add(new TiffCompressionQualityOption { CompressionQuality = (float)TiffCompressionQuality });

            if (IsPng)
                options.Add(new PngFilterModeOption { FilterOption = PngFilter });

            return options;
        }
    }
}
