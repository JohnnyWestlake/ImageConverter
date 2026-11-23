global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Runtime.InteropServices.WindowsRuntime;
global using System.Threading.Tasks;
global using System.ComponentModel;
global using System.Runtime.CompilerServices;

global using ImageConverter.Core;
global using ImageConverter.Core.CX;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ImageConverter;

sealed partial class App : Application
{
    public static Window MainWindow { get; private set; }
    public static IntPtr WindowHandle => MainWindow != null ? WinRT.Interop.WindowNative.GetWindowHandle(MainWindow) : IntPtr.Zero;
    public static DispatcherQueue Dispatcher { get; private set; }

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
    {
        Dispatcher = DispatcherQueue.GetForCurrentThread();
        MainWindow = new Window
        {
            Title = "Image Converter"
        };
        MainWindow.Content = new MainPage();
        MainWindow.Activate();
    }

}
