using TdLib;
using TgApi.Types;
using static TdLib.TdApi.MessageContent;

namespace TgApi.Telegram;

public static class TdClientExt
{
    /// <summary>
    /// Gets the user's ID from their @username
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="username">The @username of the user</param>
    /// <returns>A long of the user's ID</returns>
    public static async Task<long> GetIdFromUsernameAsync(this TdApi.Client client, string username) => 
        (await client.SearchPublicChatAsync(username)).Id;

    /// <summary>
    /// Sends a string to a user
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="id">The ID of the user</param>
    /// <param name="content">The content of the message</param>
    /// <returns>A TdApi.Message object</returns>
    public static async Task<TdApi.Message> SendBasicMessageAsync(this TdApi.Client client, long id, string content)
    {
        var input = new TdApi.InputMessageContent.InputMessageText {Text = new TdApi.FormattedText {Text = content}};
        return await client.SendMessageAsync(chatId: id, inputMessageContent: input);
    }

    /// <summary>
    /// Sends a string to a user
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="username">The @username of the user</param>
    /// <param name="content">The content of the message</param>
    /// <returns>A TdApi.Message object</returns>
    public static async Task<TdApi.Message> SendBasicMessageAsync(this TdApi.Client client, string username, string content)
    {
        long id = await client.GetIdFromUsernameAsync(username);
        return await client.SendBasicMessageAsync(id, content);
    }

    /// <summary>
    /// Sends a document to a user
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="id">The ID of the user</param>
    /// <param name="file">The file to send</param>
    /// <returns>A TdApi.Message object</returns>
    public static async Task<TdApi.Message> SendBasicDocumentAsync(this TdApi.Client client, long id, TdApi.InputFile file) =>
        await client.SendMessageAsync(chatId: id, 
            inputMessageContent:new TdApi.InputMessageContent.InputMessageDocument {Document = file});
    
    /// <summary>
    /// Sends a document to a user
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="username">The @username of the user</param>
    /// <param name="file">The file to send</param>
    /// <returns>A TdApi.Message object</returns>
    public static async Task<TdApi.Message> SendBasicDocumentAsync(this TdApi.Client client, string username, TdApi.InputFile file)
    {
        long id = await client.GetIdFromUsernameAsync(username);
        return await client.SendBasicDocumentAsync(id, file);
    }
    
    /// <summary>
    /// Gets a list of StickerPack names
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="delay">The delay between polling Telegram to wait for a new message</param>
    /// <returns>An array of string containing the names of all owned StickerPacks</returns>
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
                foreach (var b in row)
				{
                    var entry = b.Text;
                    if (entry[0] == '<') continue;
                    else packlist.Add(entry);
                }
            }
            r = packlist.ToArray();
        }
        await task;
        waiter.Close();
        Utils.Serialize<string[]>(r, $"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");
        return r;
    }

    /// <summary>
    /// Marks unread notifications in a chat as read
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <param name="chatId">The Id of the chat to mark as read</param>
    /// <returns></returns>
    public static async Task MarkChatAsRead(this TdClient client, long chatId)
	{
        var chat = await client.GetChatAsync(chatId);
        await client.ViewMessagesAsync(chatId, 0, new[] { chat.LastMessage.Id }, true);
    }

    /// <summary>
    /// Returns the text of a Message
    /// </summary>
    /// <param name="msg">A TdLib message object</param>
    /// <returns>The string content of the message</returns>
    public static string GetMessageString(this TdApi.Message msg)
	{
        if (msg.Content is MessageText txt) return txt.Text.Text;
        return "";
	}
}
