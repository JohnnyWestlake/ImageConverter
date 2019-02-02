using ImageConverter.Views;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;


namespace ImageConverter
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }
        
        public MainPage()
        {
            this.InitializeComponent();
            EnableTitleBarDrawing();
            ViewModel = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        
        private void EnableTitleBarDrawing()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var appView = ApplicationView.GetForCurrentView();
            appView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            appView.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ConvertIndex))
            {
                FilesList.ScrollIntoView(
                    FilesList.Items[ViewModel.ConvertIndex], ScrollIntoViewAlignment.Default);
            }
        }


        /* x:Bind Converters */
        public bool False(bool b) => !b;
        public bool FalseOrFalse(bool a, bool b) => !b || !a;
        public bool TrueAndFalse(bool a, bool b) => a & !b;
        public bool TrueAndTrueAndFalse(bool a, bool b, bool c) => a && b && !c;
    }
}
