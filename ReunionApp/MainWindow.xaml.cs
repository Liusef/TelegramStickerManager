using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ReunionApp
{
    public sealed partial class MainWindow : Window
    {
        public Frame ContentFrame => contentFrame;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(appTitleBar);
            SetWindowIcon("Images/AppIcon.ico");
            contentFrame.Navigate(typeof(Pages.LoadingApp));
        }

        private void SetWindowIcon(string location)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this); // Get the AppWindow from the XAML Window ("this" is your XAML window)
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(myWndId);

            appWindow.SetIcon(location); // And then set the icon
        }

        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            contentFrame.ForwardStack.Clear();
            CollectLater();
        }

        private async void CollectLater(int delay = 500)
        {
            await Task.Delay(delay);
            GC.Collect();
        }
    }
}
