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
        SetThemeShadow(InputBackground, 40, OutputBackground);
        this.Loaded += MainPage_Loaded;
    }

    private bool _startupActivationHandled;

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_startupActivationHandled)
            return;
        _startupActivationHandled = true;

        var items = new List<IStorageItem>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var activatedArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs?.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File &&
                activatedArgs.Data is Windows.ApplicationModel.Activation.IFileActivatedEventArgs fileArgs &&
                fileArgs.Files != null)
            {
                foreach (IStorageItem item in fileArgs.Files)
                {
                    if (item != null && (string.IsNullOrEmpty(item.Path) || seenPaths.Add(item.Path)))
                    {
                        items.Add(item);
                    }
                }
            }
        }
        catch
        {
        }

        try
        {
            string[] cliArgs = Environment.GetCommandLineArgs();
            for (int i = 1; i < cliArgs.Length; i++)
            {
                string arg = cliArgs[i];
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(arg);
                }
                catch
                {
                    continue;
                }

                if (!seenPaths.Add(fullPath))
                    continue;

                try
                {
                    if (File.Exists(fullPath))
                    {
                        items.Add(await StorageFile.GetFileFromPathAsync(fullPath));
                    }
                    else if (Directory.Exists(fullPath))
                    {
                        items.Add(await StorageFolder.GetFolderFromPathAsync(fullPath));
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        if (items.Count > 0)
        {
            ViewModel.ProcessDroppedFiles(items);
        }
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
