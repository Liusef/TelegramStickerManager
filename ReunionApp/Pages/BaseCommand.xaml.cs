using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TgApi.Types;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BaseCommand : Page
{

    private StickerPack pack;
    public Frame ContentFrame => Frame;

    public record BaseCommandParams(StickerPack pack, CommandType commandType);

    public enum CommandType
    {
        NONE = 0,
        ADDSTICKER = 1,
    }

    public BaseCommand()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var parameters = e.Parameter as BaseCommandParams;
        pack = parameters.pack;
        SelectPage(parameters.commandType);
    }
    
    private void SelectPage(CommandType type)
    {
        switch (type)
        {
            case CommandType.ADDSTICKER:
                ContentFrame.Navigate(typeof(CommandPages.AddSticker));
                info.Text = AddstickerInfo;
                break;
            default:
                App.GetInstance().ShowBasicDialog("No command was selected",
                    "Somehow, no command for modifying the stickerpack was selected. Please click back.");
                break;
        }
    }

    private void Back(object sender, RoutedEventArgs e) =>
        App.GetInstance().RootFrame.GoBack();


    public const string AddstickerInfo = "Haven't written instructions here yet.\n\nThis app uses Github emoji shortcodes " +
        "which differ slightly from other platforms, like discord.";
}
