namespace ImageConverter.Core.CX;

public enum BitmapFrameHandling
{
    /// <summary>
    /// Only the first frame of the bitmap is converted
    /// </summary>
    FirstFrameOnly = 0,

    /// <summary>
    /// All frames of the bitmap are converted to separate files
    /// </summary>
    ExtractAllSeperately = 1
}
