using ImageConverter.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System.Numerics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace ImageConverter;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }
    public UISettings UISettings { get; }

    public MainPage()
    {
        this.InitializeComponent();

        if (DesignMode.DesignModeEnabled)
            return;

        UISettings = new UISettings();
        UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        UdpateTitleBar();

        ViewModel = new MainViewModel();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        SetThemeShadow(InputBackground, 40, OutputBackground);
    }

    private void UISettings_ColorValuesChanged(UISettings sender, object args)
    {
        App.Dispatcher?.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.High, UdpateTitleBar);
    }

    private void UdpateTitleBar()
    {
        try
        {
            if (App.MainWindow == null)
                return;

            App.MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(TitleBarBackground);
            if (App.MainWindow.AppWindow?.TitleBar is { } titleBar)
            {
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                if (Application.Current.Resources.TryGetValue("ApplicationForegroundThemeBrush", out var resource) && resource is SolidColorBrush brush)
                {
                    titleBar.ButtonForegroundColor = brush.Color;
                    titleBar.ButtonHoverForegroundColor = brush.Color;
                }
            }
        }
        catch { }
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ConvertIndex))
        {
            FilesList.ScrollIntoView(
                FilesList.Items[ViewModel.ConvertIndex], ScrollIntoViewAlignment.Default);
        }
    }

    private void Output_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems)
            )//&& !e.DataView.AvailableFormats.Contains("FileOpFlags")) // Removes special environment folders that don't "drop"
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.Caption = "Set output folder";
        }
    }

    private async void Output_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
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

    private void FilesList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue)
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? UnBandedBrush : BandedBrush;
    }

    private void Files_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.Caption = "Add files";
        }
    }

    private async void Files_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            ViewModel.ProcessDroppedFiles(items);
        }
    }

    public static void SetThemeShadow(UIElement target, float depth, params UIElement[] recievers)
    {
        try
        {
            target.Translation = new Vector3(0, 0, depth);

            var shadow = new ThemeShadow();
            target.Shadow = shadow;
            foreach (var r in recievers)
                shadow.Receivers.Add(r);
        }
        catch { }
    }

    private void ShowHelp() => HelpToolTip.IsOpen = true;

    private void FocusAddFiles() => FocusTarget(AddFilesButton);

    private void FocusOutputFolder(Hyperlink sender, HyperlinkClickEventArgs args) => FocusTarget(OutputFolderButton);

    private void FocusOutputFormat(Hyperlink sender, HyperlinkClickEventArgs args) => FocusTarget(OutputFormatSelector);

    private void FocusFormatOptions(Hyperlink sender, HyperlinkClickEventArgs args) => FocusTarget(OutputFormatOptionsButton);

    private void FocusResize(Hyperlink sender, HyperlinkClickEventArgs args) => FocusTarget(ResizeOptionsButton);

    private void FocusTarget(Control c) => c.Focus(FocusState.Keyboard);


    /* x:Bind Converters */
    public bool False(bool b) => !b;
    public bool FalseOrFalse(bool a, bool b) => !b || !a;
    public bool TrueAndFalse(bool a, bool b) => a & !b;
    public bool TrueAndTrueAndFalse(bool a, bool b, bool c) => a && b && !c;
    public Visibility FalseToVis(bool a) => a ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility NullOrEmptyToVis(string s) => string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
}
