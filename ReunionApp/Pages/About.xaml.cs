using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ReunionApp.Pages;

public record Attribution(string Name, string Version, string Copyright, string License, Uri link);

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class About : Page
{
    private Attribution[] attributions =
    {
        new(".NET Core"             , "6.0.2"   , "©️ .NET Foundation"           , "MIT License"         , new("https://github.com/dotnet/core")),
        new("Windows App SDK"       , "1.0.1"   , "Microsoft Corporation"       , "Creative Commons 4.0", new("https://github.com/microsoft/WindowsAppSDK")),
        new("microsoft-ui-xaml"     , "WinUI 3" , "©️ Microsoft Corporation"     , "MIT License"         , new("https://github.com/microsoft/microsoft-ui-xaml")),
        new("MSTest"                , "2.2.8"   , "©️ Microsoft Corporation"     , "MIT License"         , new("https://github.com/microsoft/testfx")),
        new("coverlet"              , "3.1.2"   , "©️ 2018 Toni Solarin-Sodara"  , "MIT License"         , new("https://github.com/coverlet-coverage/coverlet")),
        new("TDLib"                 , "1.7.9.1" , "Telegram Messenger"          , "BSL 1.0"             , new("https://github.com/tdlib/td")),
        new("TDSharp"               , "1.7.9"   , "©️ 2018 Sergey Khabibullin"   , "MIT License"         , new("https://github.com/egramtel/tdsharp")),
        new("Newtonsoft.Json"       , "13.0.1"  , "©️ 2007 James Newton-King"    , "MIT License"         , new("https://github.com/JamesNK/Newtonsoft.Json")),
        new("libphonenumber"        , "8.12.45" , "Google LLC"                  , "Apache-2.0 License"  , new("https://github.com/google/libphonenumber")),
        new("libphonenumber-csharp" , "8.12.45" , "twcclegg"                    , "Apache-2.0 License"  , new("https://github.com/twcclegg/libphonenumber-csharp")),
        new("ImageSharp"            , "2.1.0"   , "©️ SixLabors"                 , "Apache-2.0 License"  , new("https://github.com/SixLabors/ImageSharp")),
        new("Unicode.NET"           , "2.0.0"   , "©️ Neosmart"                  , "MIT License"         , new("https://github.com/neosmart/unicode.net")),
        new("GEmojiSharp"           , "1.5.0"   , "©️ 2019 Henrik Lau Eriksson"  , "MIT License"         , new("https://github.com/hlaueriksson/GEmojiSharp")),
    };


    public About()
    {
        this.InitializeComponent();
    }

    private void Back(object sender, RoutedEventArgs e) => App.GetInstance().RootFrame.GoBack();
}
