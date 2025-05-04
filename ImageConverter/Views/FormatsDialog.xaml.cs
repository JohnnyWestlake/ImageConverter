using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        Border b = (Border)this.GetTemplateChild("BackgroundElement");
        b.Margin = new Thickness(40);
    }

    internal static void Show(CodecSupport support)
    {
        _ = (new FormatsDialog(support)).ShowAsync();
    }

    public Visibility FalseToVis(bool a) => a ? Visibility.Collapsed : Visibility.Visible;

}
