using ImageConverter.Core;
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
    public sealed partial class OptionsDialog : ContentDialog
    {
        public OptionsViewModel ViewModel { get; }

        public OptionsDialog(OptionsViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
        }

        public void Close()
        {
            this.Bindings.StopTracking();
            this.Hide();
        }
    }
}
