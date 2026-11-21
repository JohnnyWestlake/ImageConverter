using Windows.UI.Popups;

namespace ImageConverter.Common;

public static class MessageBox
{
    public static void Show(String message, String title)
    {
        try
        {
            var md = new MessageDialog(message, title);
            if (App.WindowHandle != IntPtr.Zero)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(md, App.WindowHandle);
            }
            _ = md.ShowAsync();
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
        }
    }
}
