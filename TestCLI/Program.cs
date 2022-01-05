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

Console.WriteLine(JsonSerializer.Serialize<string[]>(await client.GetOwnedPacksAsync()));

var sset = await client.SearchStickerSetAsync("TegraFoxStickers");

var dls = new List<FileDownload>();
foreach (var ssetSticker in sset.Stickers) dls.Add(await FileDownload.StartDownload(client, ssetSticker.Sticker_.Id));
foreach (var fileDownload in dls)
{
    await fileDownload.WaitForCompletion();
}

