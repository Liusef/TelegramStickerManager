using TdLib;

namespace TgApi.Telegram;

public class MessageWaiter
{
	private TdClient client;
	private long chatId;
	private EventHandler<TdApi.Update> handler;
	private TdApi.Message? lastMsg;

	/// <summary>
	/// Instantiates a MessageWaiter Object
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="chatId">The chat ID to wait in</param>
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

	/// <summary>
	/// Waits for a new message
	/// </summary>
	/// <param name="currId">The ID of the current message</param>
	/// <param name="delay">The delay between polling Telegram for a new message</param>
	/// <returns>A TdApi.Message object</returns>
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

	/// <summary>
	/// Waits for a new message that contains a string
	/// </summary>
	/// <param name="target">The target string content to look for</param>
	/// <param name="delay">THe delay between polling Telegram for a new message</param>
	/// <returns>A TdApi.MessageObject</returns>
	public async Task<TdApi.Message> WaitForMsgAsync(string target, int delay = 25)
	{
		var t = DateTime.Now;
		while (!target.Equals(
				   (lastMsg.Content as TdApi.MessageContent.MessageText ??
				   new TdApi.MessageContent.MessageText { Text = new TdApi.FormattedText { Text = "" } })
				   .Text.Text)) // Ensuring no NullReference using ??
		{
			await Task.Delay(delay);
		}
		Console.WriteLine($"Elapsed: { (DateTime.Now - t).TotalMilliseconds }ms");
		return lastMsg;
	}

	/// <summary>
	/// Sends a message and waits for the next message to be sent
	/// </summary>
	/// <param name="contents">The content of the message to send</param>
	/// <param name="delay">The delay between polling Telegram for a new message</param>
	/// <returns>A TdApi.Message object</returns>
	public async Task<TdApi.Message> SendMsgAndAwaitNext(string contents, int delay = 25)
	{
		long id = (await client.SendBasicMessageAsync(chatId, contents)).Id;
		return await WaitNextMsgAsync(id, delay);
	}

	/// <summary>
	/// Sends a message and waits for the next message to be sent
	/// </summary>
	/// <param name="contents">The content of the message to send</param>
	/// <param name="target">The target string content to look for</param>
	/// <param name="delay">The delay between polling Telegram for a new message</param>
	/// <returns>A TdApi.Message object</returns>
	public async Task<TdApi.Message> SendMsgAndAwaitNext(string contents, string target, int delay = 25)
	{
		await client.SendBasicMessageAsync(chatId, contents);
		return await WaitForMsgAsync(target, delay);
	}

	/// <summary>
	/// Gets rid of references so that the garbage collector hopefully gets rid of stuff
	/// </summary>
	public void Close()
	{
		this.client.UpdateReceived -= this.handler;
		this.client = null;
		this.chatId = default;
		this.lastMsg = null;
		this.handler = null;
	}

}
