using TdLib;
using static TdLib.TdApi;
using static TdLib.TdApi.InputMessageContent;

namespace TgApi.Telegram;

public static class Messages
{
    public static async Task<Message> SendBasicMessageAsync(this Client client, long id, string content)
    {
        var input = new InputMessageText {Text = new FormattedText {Text = content}};
        return await client.SendMessageAsync(chatId: id, inputMessageContent: input);
    }

    public static async Task<Message> SendBasicMessageAsync(this Client client, string username, string content)
    {
        long id = await client.GetIdFromUsernameAsync(username);
        return await client.SendBasicMessageAsync(id, content);
    }
}
