using TdLib;
using TgApi.Telegram;

var client = new TdClient();
client.Bindings.SetLogVerbosityLevel(0);

var auth = new AuthHandler(client);

await auth.SigninCLI();
                  
