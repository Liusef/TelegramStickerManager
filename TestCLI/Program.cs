using TdLib;
using TgApi.Telegram;

var client = new TdClient();
client.Bindings.SetLogVerbosityLevel(4);

var auth = new AuthHandler(client);

await auth.SigninCLI();
auth.Close();

var sset = await client.SearchStickerSetAsync("TegraFoxStickers");

foreach (TdApi.Sticker s in sset.Stickers)
{
    Console.WriteLine($"Downloading Sticker | ID: {s.Sticker_.Id}, Emojis: {s.Emoji}");
    var file = await client.DownloadFileAsync(fileId: s.Sticker_.Id, priority:16);
    Console.WriteLine(file.Local.Path);
    await Task.Delay(100);
}

                  
