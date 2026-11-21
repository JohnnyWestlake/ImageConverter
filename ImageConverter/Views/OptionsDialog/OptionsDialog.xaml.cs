using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ImageConverter.Views;

public sealed partial class OptionsDialog : ContentDialog
{
    public OptionsViewModel ViewModel { get; }

    private OptionsDialog(OptionsViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
    }

    public static void Show(OptionsViewModel viewModel)
    {
        var dialog = new OptionsDialog(viewModel)
        {
            XamlRoot = App.MainWindow?.Content?.XamlRoot
        };
        _ = dialog.ShowAsync();
    }

    public void Close()
    {
        this.Bindings.StopTracking();
        this.Hide();
    }

    private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.GetTemplateChild("BackgroundElement") is Border b)
        {
            b.Margin = new Thickness(40);
        }
    }
}
