using System.Text.Json;
using TdLib;
using TgApi.Types;
using static TdLib.TdApi;

namespace TgApi.Telegram;

public static class Stickers
{
    public static async Task<string[]> GetOwnedPacksAsync(this TdClient client, int delay = 100)
    {
        long id = await client.GetIdFromUsernameAsync(GlobalVars.StickerBot);
        var waiter = new MessageWaiter(client, id);
        await waiter.SendMsgAndAwaitNext("/cancel");
        var msg = await waiter.SendMsgAndAwaitNext("/addsticker");
        var task = waiter.SendMsgAndAwaitNext("/cancel");
        string[] r = new string[0];
        if (msg.ReplyMarkup is ReplyMarkup.ReplyMarkupShowKeyboard rmsk)
        {
            var packlist = new List<string>();
            foreach (var row in rmsk.Rows)
            {
                foreach (var b in row) packlist.Add(b.Text);
            }
            r = packlist.ToArray();
        }
        await task;
        waiter.Close();
        return r;
    }
}
