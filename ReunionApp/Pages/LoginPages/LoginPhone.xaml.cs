using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TgApi.Telegram;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages.LoginPages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPhone : Page
{

    private AuthHandler auth;

    public LoginPhone()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        auth = e.Parameter as AuthHandler;
    }

    private void PhoneNumberBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ContinueButton.IsEnabled = TgApi.Utils.IsValidPhone("+" + PhoneNumberBox.Text.Trim());
    }

    private async Task TgSubmitPhone(string phoneNumber)
    {
        var last = auth.LastRequestReceivedAt;
        App.GetInstance().RootFrame.Navigate(typeof(LoginCode),
                new LoginCode.LoginCodeParams(auth, TgApi.Utils.FormatPhone("+" + phoneNumber.Trim())),
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        try
        {
            await auth.Handle_WaitPhoneNumber(phoneNumber);
        }
        catch (Exception ex)
        {
            App.GetInstance().ShowExceptionDialog(ex);
            App.GetInstance().RootFrame.Navigate(typeof(Pages.GenericError));
        }

        while (last == auth.LastRequestReceivedAt) await Task.Delay(50);

        LoginCode.IsNotSupportedState(AuthHandler.GetState(auth.CurrentState));
    }

    private async void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        await TgSubmitPhone(PhoneNumberBox.Text.Trim());
    }

    private async void PhoneNumberBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ContinueButton.IsEnabled)
            await TgSubmitPhone(PhoneNumberBox.Text.Trim());
    }
}
