using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TdLib;
using TDLib.Bindings;
using TgApi;
using TgApi.Telegram;
using TgApi.Types;

Console.OutputEncoding = Encoding.Unicode;

GlobalVars.EnsureDirectories();

var client = GlobalVars.Client;
client.Bindings.SetLogVerbosityLevel(0);
client.Bindings.SetLogFilePath("what");

var auth = new AuthHandler(client);

await auth.SigninCLI();
auth.Close();

string packname = "RichardYohan";
string username = "Genericcanine";

var sset = await client.SearchStickerSetAsync(packname);
await client.SendBasicMessageAsync(username, "testing code rn, sry for spam");

for (int i = 0; i < sset.Stickers.Length; i++)
{
    await client.SendBasicDocumentAsync(username, new TdApi.InputFile.InputFileRemote{Id = sset.Stickers[i].Sticker_.Remote.Id});
    await Task.Delay(20);
}
await Task.Delay(5000);
