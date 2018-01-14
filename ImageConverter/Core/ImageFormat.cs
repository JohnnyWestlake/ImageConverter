using ImageConverter.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Imaging;

namespace ImageConverter.Core
{
    public class ImageFormat
    {
        public BitmapCodecInformation CodecInfo { get; }
        public string DisplayName { get; }
        public string DefaultFileExtension { get; }

        public ImageFormat(BitmapCodecInformation info)
        {
            CodecInfo = info;
            DisplayName = ImageConverterCore.GetPrefferedDisplayName(info);
            DefaultFileExtension = ImageConverterCore.GetPrefferedFileExtension(info);
        }
    }
}
