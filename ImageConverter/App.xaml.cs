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

using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ImageConverter;

sealed partial class App : Application
{
    public App()
    {
        this.FocusVisualKind = FocusVisualKind.Reveal;
        this.InitializeComponent();
        this.UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
    }

    public static CoreDispatcher Dispatcher { get; private set; }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (!(Window.Current.Content is MainPage page))
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            Dispatcher = Window.Current.Dispatcher;
            page = new MainPage();

            // Place the frame in the current Window
            Window.Current.Content = page;
        }

        if (e.PrelaunchActivated == false)
        {
            Window.Current.Activate();
        }
    }

}
