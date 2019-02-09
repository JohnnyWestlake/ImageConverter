using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ImageConverter.Common
{
    public static class MessageBox
    {
        public static void Show(String message, String title)
        {
            try
            {
                var md = new MessageDialog(message, title);
                _ = md.ShowAsync();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
