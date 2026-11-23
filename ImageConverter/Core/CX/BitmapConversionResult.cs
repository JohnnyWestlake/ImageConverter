namespace ImageConverter.Core.CX;

public sealed class BitmapConversionResult
{
    public bool Success { get; set; }

    public ulong ResultFileSize { get; set; }

    public string Status { get; set; }

    public BitmapConversionResult() { }

    public BitmapConversionResult(bool success, string status)
    {
        Success = success;
        Status = status;
    }
}
