using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        _ = (new OptionsDialog(viewModel)).ShowAsync();
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
}
