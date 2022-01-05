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

var u = await FileUpload.StartUpload(client, "C:\\Users\\josep\\Pictures\\100017500389_71978.jpg");
await u.WaitForCompletion();

await client.SendBasicDocumentAsync("aRandomFox",new TdApi.InputFile.InputFileId {Id = u.LocalId});