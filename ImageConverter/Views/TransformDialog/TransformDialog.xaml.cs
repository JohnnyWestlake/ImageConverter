using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ImageConverter.Views
{
    public sealed partial class TransformDialog : ContentDialog
    {
        public TransformViewModel ViewModel { get; }

        private TransformDialog(TransformViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
        }

        public static void Show(TransformViewModel viewModel)
        {
            _ = (new TransformDialog(viewModel)).ShowAsync();
        }

        public void Close()
        {
            this.Bindings.StopTracking();
            this.Hide();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Border b = (Border)this.GetTemplateChild("BackgroundElement");
            b.Margin = new Thickness(40);
        }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (args.NewText.Any(c => !(((c >= '0') && (c <= '9')))))
                args.Cancel = true;
        }
    }
}
