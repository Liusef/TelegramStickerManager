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
GlobalVars.Client = new TdClient();

var client = GlobalVars.Client;
client.Bindings.SetLogVerbosityLevel(0);

var auth = new AuthHandler(client);

await auth.SigninCLI();
