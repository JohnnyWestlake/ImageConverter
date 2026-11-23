using Windows.Graphics.Imaging;

namespace ImageConverter.Core.CX;

public sealed class BitmapOption
{
    public BitmapOption(string name, BitmapTypedValue bitmapValue)
    {
        Value = bitmapValue;
        Name = name;
    }

    public BitmapTypedValue Value { get; set; }

    public string Name { get; set; }
}
