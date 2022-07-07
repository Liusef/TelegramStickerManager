using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ReunionApp;

/// <summary>
/// The class that describes the MainWindow of the applciation
/// </summary>
public sealed partial class MainWindow : Window
{
    public Frame ContentFrame => MainWindowContentFrame;

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "Telegram Stickers";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetWindowIcon("Images/AppIcon.ico");
        ContentFrame.Navigate(typeof(Pages.LoadingApp));
    }

    /// <summary>
    /// Sets the window icon to a string path. Must be an ICO file
    /// </summary>
    /// <param name="location"></param>
    private void SetWindowIcon(string location)
    {
        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(myWndId); // Get AppWindow obj of this

        appWindow.SetIcon(location); // And then set the icon
    }

    /// <summary>
    /// An action to be run whenever the main content frame navigates to a new location
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void contentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ContentFrame.ForwardStack.Clear();
        AppUtils.CollectLater(2000);
    }


}
