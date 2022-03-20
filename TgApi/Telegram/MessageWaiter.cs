using TdLib;
using static TdLib.TdApi.InputFile;

namespace TgApi.Telegram;

public class MessageWaiter
{
	private TdClient _client;
	private long _chatId;
	private EventHandler<TdApi.Update> _handler;
	private TdApi.Message? _lastMsg;

	/// <summary>
	/// Instantiates a MessageWaiter Object
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="chatId">The chat ID to wait in</param>
	public MessageWaiter(TdClient client, long chatId)
	{
		_client = client;
		_chatId = chatId;
		_lastMsg = null;

		_handler = (sender, update) =>
		{
			if (update is TdApi.Update.UpdateNewMessage msgUpdate && msgUpdate.Message.ChatId == _chatId)
			{
				_lastMsg = msgUpdate.Message;
			}
		};
		client.UpdateReceived += _handler;
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
		while (currId == _lastMsg.Id)
		{
			await Task.Delay(delay);
		}
		Console.WriteLine($"Elapsed: { (DateTime.Now - t).TotalMilliseconds }ms");
		return _lastMsg;
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
				   (_lastMsg?.Content as TdApi.MessageContent.MessageText ??
				   new TdApi.MessageContent.MessageText { Text = new TdApi.FormattedText { Text = "" } })
				   .Text.Text)) // Ensuring no NullReference using ??
		{
			await Task.Delay(delay);
		}
		Console.WriteLine($"Elapsed: { (DateTime.Now - t).TotalMilliseconds }ms");
		return _lastMsg;
	}

	/// <summary>
	/// Sends a message and waits for the next message to be sent
	/// </summary>
	/// <param name="contents">The content of the message to send</param>
	/// <param name="delay">The delay between polling Telegram for a new message</param>
	/// <returns>A TdApi.Message object</returns>
	public async Task<TdApi.Message> SendMsgAndAwaitNext(string contents, int delay = 25)
	{
		long id = (await _client.SendBasicMessageAsync(_chatId, contents)).Id;
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
		await _client.SendBasicMessageAsync(_chatId, contents);
		return await WaitForMsgAsync(target, delay);
	}

    /// <summary>
    /// Sends a document and waits for the next message to be sent
    /// </summary>
    /// <param name="input">The InputFileId corresponding to the document</param>
    /// <param name="delay">The delay between polling Telegram for a new message</param>
    /// <returns>A TdApi.Message object</returns>
    public async Task<TdApi.Message> SendDocumentAndAwaitNext(InputFileId input, int delay = 25)
    {
        long id = (await _client.SendBasicDocumentAsync(_chatId, input)).Id;
        return await WaitNextMsgAsync(id, delay);
    }

	/// <summary>
	/// Gets rid of references so that the garbage collector hopefully gets rid of stuff
	/// </summary>
	public void Close()
	{
		this._client.UpdateReceived -= this._handler;
		this._client = null;
		this._chatId = default;
		this._lastMsg = null;
		this._handler = null;
	}

}
