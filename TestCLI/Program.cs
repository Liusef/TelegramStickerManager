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

// await client.OptimizeStorageAsync();

void sdsTest<T>(T obj)
{
    var start = DateTime.Now;
    var path = "C:\\users\\josep\\Desktop\\test.json";
    Utils.Serialize<T>(obj, path);
    Utils.Deserialize<T>(path);
    Console.WriteLine($"Serialization Benchmark took {(DateTime.Now - start).TotalMilliseconds}ms");
}

var sset = await StickerPack.GenerateFromName(client, "RichardYohan");

sdsTest<StickerPack>(sset);
sset.Stickers = null;
sdsTest<StickerPack>(sset);