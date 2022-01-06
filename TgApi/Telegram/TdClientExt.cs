using TdLib;
using TgApi.Types;

namespace TgApi.Telegram;

public static class TdClientExt
{
    public static async Task<long> GetIdFromUsernameAsync(this TdApi.Client client, string username) => 
        (await client.SearchPublicChatAsync(username)).Id;

    public static async Task<TdApi.Message> SendBasicMessageAsync(this TdApi.Client client, long id, string content)
    {
        var input = new TdApi.InputMessageContent.InputMessageText {Text = new TdApi.FormattedText {Text = content}};
        return await client.SendMessageAsync(chatId: id, inputMessageContent: input);
    }

    public static async Task<TdApi.Message> SendBasicMessageAsync(this TdApi.Client client, string username, string content)
    {
        long id = await client.GetIdFromUsernameAsync(username);
        return await client.SendBasicMessageAsync(id, content);
    }

    public static async Task<TdApi.Message> SendBasicDocumentAsync(this TdApi.Client client, string username, TdApi.InputFile file)
    {
        long id = await client.GetIdFromUsernameAsync(username);
        return await client.SendMessageAsync(chatId: id, 
            inputMessageContent:new TdApi.InputMessageContent.InputMessageDocument {Document = file});
    }
    
    public static async Task<string[]> GetOwnedPacksAsync(this TdClient client, int delay = 100)
    {
        long id = await client.GetIdFromUsernameAsync(GlobalVars.StickerBot);
        var waiter = new MessageWaiter(client, id);
        await waiter.SendMsgAndAwaitNext("/cancel");
        var msg = await waiter.SendMsgAndAwaitNext("/addsticker");
        var task = waiter.SendMsgAndAwaitNext("/cancel");
        string[] r = new string[0];
        if (msg.ReplyMarkup is TdApi.ReplyMarkup.ReplyMarkupShowKeyboard rmsk)
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
