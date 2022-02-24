using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ReunionApp;

public sealed partial class MainWindow : Window
{
    public Frame ContentFrame => MainWindowContentFrame;

    public MainWindow()
    {
        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetWindowIcon("Images/AppIcon.ico");
        ContentFrame.Navigate(typeof(Pages.LoadingApp));
    }

    private void SetWindowIcon(string location)
    {
        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this); // Get the AppWindow from the XAML Window ("this" is your XAML window)
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(myWndId);

        appWindow.SetIcon(location); // And then set the icon
    }

    private void contentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ContentFrame.ForwardStack.Clear();
        CollectLater();
    }

    private static async void CollectLater(int delay = 500)
    {
        await Task.Delay(delay);
        GC.Collect();
    }
}
