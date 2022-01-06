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

void sdsTest(TdClient client, string name)
{
    var start = DateTime.Now;
    var pack = Utils.Deserialize<StickerPack>($"{GlobalVars.PacksDir}{name}.json");
    Console.WriteLine($"Deserialization Benchmark took {(DateTime.Now - start).TotalMilliseconds}ms, Name: {pack.Name}");
}

foreach (string s in await client.GetOwnedPacksAsync())
{
    sdsTest(client, s);
}
