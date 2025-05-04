﻿using ImageConverter.Views;
using System.Numerics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace ImageConverter;

public sealed partial class MainPage : Page
{
    public static bool Supports1903 { get; } = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8);

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
        _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, UdpateTitleBar);
    }

    private void UdpateTitleBar()
    {
        CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
        Window.Current.SetTitleBar(TitleBarBackground);
        var appView = ApplicationView.GetForCurrentView();
        appView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        appView.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        appView.TitleBar.ButtonForegroundColor = ((SolidColorBrush)App.Current.Resources["ApplicationForegroundThemeBrush"]).Color;
        appView.TitleBar.ButtonHoverForegroundColor = ((SolidColorBrush)App.Current.Resources["ApplicationForegroundThemeBrush"]).Color;
    }

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

    private void FilesList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (!args.InRecycleQueue)
            args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? UnBandedBrush : BandedBrush;
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

    public static void SetThemeShadow(UIElement target, float depth, params UIElement[] recievers)
    {
        try
        {
            if (!Supports1903 || !CompositionCapabilities.GetForCurrentView().AreEffectsFast())
                return;

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
