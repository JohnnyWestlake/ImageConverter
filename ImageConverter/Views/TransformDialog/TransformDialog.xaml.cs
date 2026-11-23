using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ImageConverter.Views;

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
        var dialog = new TransformDialog(viewModel)
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

    private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (args.NewText.Any(c => !(((c >= '0') && (c <= '9')))))
            args.Cancel = true;
    }
}
