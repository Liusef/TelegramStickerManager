using TdLib;
using TgApi.Telegram;

namespace TgApi.Types;

public class MessageWaiter
{
    private TdClient client;
    private long chatId;
    private EventHandler<TdApi.Update> handler;
    private TdApi.Message lastMsg;

    public MessageWaiter(TdClient client, long chatId)
    {
        this.client = client;
        this.chatId = chatId;
        this.lastMsg = null;

        this.handler = (sender, update) =>
        {
            if (update is TdApi.Update.UpdateNewMessage msgUpdate && msgUpdate.Message.ChatId == this.chatId)
            {
                lastMsg = msgUpdate.Message;
            }
        };
        client.UpdateReceived += handler;
    }

    public async Task<TdApi.Message> WaitNextMsgAsync(long currId, int delay = 25)
    {
        var t = DateTime.Now;
        while (currId == lastMsg.Id)
        {
            await Task.Delay(delay);
        }
        Console.WriteLine($"Elapsed: { (DateTime.Now - t).TotalMilliseconds }ms");
        return lastMsg;
    }

    public async Task<TdApi.Message> SendMsgAndAwaitNext(string contents, int delay = 25)
    {
        long id = (await client.SendBasicMessageAsync(chatId, contents)).Id;
        return await WaitNextMsgAsync(id, delay);
    }

    public void Close()
    {
        this.client.UpdateReceived -= this.handler;
        this.client = null;
        this.chatId = default;
        this.lastMsg = null;
        this.handler = null;
    }

}
