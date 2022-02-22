using System.Text;
using TdLib;
using TgApi;
using TgApi.Telegram;
using TgApi.Types;

Console.OutputEncoding = Encoding.Unicode;

GlobalVars.EnsureDirectories();

var client = new TdClient();
client.Bindings.SetLogVerbosityLevel(0);

var auth = new AuthHandler(client);

await auth.SigninCLI();

var pack = await StickerPack.GenerateFromName(client, "RichardYohan");

foreach (var s in pack.Stickers)
{
	await s.GetPathEnsureDownloaded(client);
	Console.WriteLine(s.Emojis + "\n");
}

Console.WriteLine("Done!");


