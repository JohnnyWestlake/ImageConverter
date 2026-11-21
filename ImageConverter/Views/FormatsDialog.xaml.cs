using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ImageConverter.Views;

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
        if (this.GetTemplateChild("BackgroundElement") is Border b)
        {
            b.Margin = new Thickness(40);
        }
    }

    internal static void Show(CodecSupport support)
    {
        var dialog = new FormatsDialog(support)
        {
            XamlRoot = App.MainWindow?.Content?.XamlRoot
        };
        _ = dialog.ShowAsync();
    }

    public Visibility FalseToVis(bool a) => a ? Visibility.Collapsed : Visibility.Visible;

}
