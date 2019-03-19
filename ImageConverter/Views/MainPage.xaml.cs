using ImageConverter.Views;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;


namespace ImageConverter
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        public UISettings UISettings { get; }

        public MainPage()
        {
            this.InitializeComponent();
            //EnableTitleBarDrawing();
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
            DisableTitleBarDrawing();

            ViewModel = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, DisableTitleBarDrawing);
        }

        private void DisableTitleBarDrawing()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            var appView = ApplicationView.GetForCurrentView();
            appView.TitleBar.ButtonBackgroundColor = (Color)App.Current.Resources["SystemChromeMediumColor"];
            appView.TitleBar.ButtonInactiveBackgroundColor = (Color)App.Current.Resources["SystemChromeLowColor"];
            appView.TitleBar.BackgroundColor = (Color)App.Current.Resources["SystemChromeMediumColor"];
            appView.TitleBar.InactiveBackgroundColor = (Color)App.Current.Resources["SystemChromeLowColor"];
        }


        //private void EnableTitleBarDrawing()
        //{
        //    CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
        //    var appView = ApplicationView.GetForCurrentView();
        //    appView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        //    appView.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        //}

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ConvertIndex))
            {
                FilesList.ScrollIntoView(
                    FilesList.Items[ViewModel.ConvertIndex], ScrollIntoViewAlignment.Default);
            }
        }

        private void Output_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)
                )//&& !e.DataView.AvailableFormats.Contains("FileOpFlags")) // Removes special environment folders that don't "drop"
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.Caption = "Set output folder";
            }
        }

        private async void Output_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.OfType<StorageFolder>().FirstOrDefault() is StorageFolder folder)
                {
                    ViewModel.ProcessExportFolder(folder);
                }
            }
        }

        private void Files_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.Caption = "Add files";
            }
        }

        private async void Files_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                ViewModel.ProcessDroppedFiles(items);
            }
        }



        /* x:Bind Converters */
        public bool False(bool b) => !b;
        public bool FalseOrFalse(bool a, bool b) => !b || !a;
        public bool TrueAndFalse(bool a, bool b) => a & !b;
        public bool TrueAndTrueAndFalse(bool a, bool b, bool c) => a && b && !c;
    }
}
