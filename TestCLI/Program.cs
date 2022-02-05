using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TdLib;
using TDLib.Bindings;
using TgApi;
using TgApi.Telegram;
using TgApi.Types;

Console.OutputEncoding = Encoding.Unicode;

// GlobalVars.EnsureDirectories();
// GlobalVars.Client = new TdClient();
//
// var client = GlobalVars.Client;
// client.Bindings.SetLogVerbosityLevel(0);
//
// var auth = new AuthHandler(client);
//
// await auth.SigninCLI();
//
// var pack = await StickerPack.GenerateFromName(client, "RichardYohan");
//
// foreach (var s in pack.Stickers)
// {
//     await s.GetPathEnsureDownloaded(client);
// }

var path = "C:\\Users\\josep\\Stuff\\VP228 - Copy.png";
using (var img = SixLabors.ImageSharp.Image.Load(path))
{
    Console.WriteLine($"This image is {img.Width}x{img.Height} with pixel format {img.PixelType}");
}
