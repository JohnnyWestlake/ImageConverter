using ImageConverter.Core.CX;
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
    public sealed partial class FormatsDialog : ContentDialog
    {
        CodecSupport _support { get; }

        public FormatsDialog(CodecSupport support)
        {
            _support = support;
            this.InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Border b = (Border)this.GetTemplateChild("BackgroundElement");
            b.Margin = new Thickness(40);
        }

        internal static void Show(CodecSupport support)
        {
            _ = (new FormatsDialog(support)).ShowAsync();
        }

        public Visibility FalseToVis(bool a) => a ? Visibility.Collapsed : Visibility.Visible;

    }
}
