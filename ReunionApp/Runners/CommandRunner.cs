using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TdLib;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi;

namespace ReunionApp.Runners;

public abstract class CommandRunner : INotifyPropertyChanged
{
    protected StickerPack pack;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CommandOutput> Outputs { get; set; } = new();

    private double _progress = 0;
    public double Progress
    {
        get { return _progress; }
        set
        {
            _progress = value;
            OnProgressChanged();
        }
    }

    /// <summary>
    /// This method is called when the value of progress is updated
    /// </summary>
    protected void OnProgressChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
    }

    /// <summary>
    /// Runs the commands queued in the runner
    /// </summary>
    /// <returns>An awaitable task</returns>
    public abstract Task RunCommandsAsync();

    /// <summary>
    /// Actions to perform before the execution of queued commands
    /// </summary>
    /// <returns>An awaitable task</returns>
    public virtual async Task PreTasksAsync()
    {
        await Task.Run (async () => await Task.Delay(0)); // This is a placeholder
    }

    /// <summary>
    /// Actions to perform after the execution of queued commands
    /// </summary>
    /// <returns>An awaitable task</returns>
    public virtual async Task PostTasksAsync()
    {
        var client = App.GetInstance().Client;
        var chat = await client.GetIdFromUsernameAsync("Stickers");
        await client.MarkChatAsRead(chat);
        if (pack != null) pack.IsCachedCopy = true;
    }

    /// <summary>
    /// Adds a TDLib string message to the reply
    /// </summary>
    /// <param name="msg">The Message object to add the outputs list</param>
    protected virtual void AddReplyToOutputs(Message msg) =>
        Outputs.Add(new CommandOutput(msg.GetMessageString(), null, false));

    /// <summary>
    /// Send a specified message to a Telegram user (specified within the MessageWaiter) and adds the reply to the outputs list
    /// </summary>
    /// <param name="waiter">The MessageWaiter object in question</param>
    /// <param name="message">The message (as a string) to send</param>
    /// <returns>The reply as a string</returns>
    protected virtual async Task<string> SendAndAddToOutputsAsync(MessageWaiter waiter, string message)
    {
        Outputs.Add(new CommandOutput(message, null, true));
        var reply = await waiter.SendMsgAndAwaitNext(message);
        AddReplyToOutputs(reply);
        return reply.GetMessageString();
    }

    /// <summary>
    /// Sends an image and add the reply to the outputs
    /// </summary>
    /// <param name="waiter">The MessageWaiter in question</param>
    /// <param name="path">The image input path to send to telegram</param>
    /// <returns>The reply as a string</returns>
    protected virtual async Task<string> SendImageAndAddToOutputsAsync(MessageWaiter waiter, string path)
    {
        Outputs.Add(new CommandOutput(null, path, true));
        var upload = await FileUpload.StartUpload(App.GetInstance().Client, path);
        await upload.WaitForCompletion();
        var r = await waiter.SendDocumentAndAwaitNext(new InputFile.InputFileId { Id = upload.LocalId });
        AddReplyToOutputs(r);
        return r.GetMessageString();
    }   

    /// <summary>
    /// Sends a document and adds the reply to the outputs
    /// </summary>
    /// <param name="waiter">The MessageWaiter in question</param>
    /// <param name="input">The image input path to send to telegram</param>
    /// <param name="display">The commandoutput to display to the user</param>
    /// <returns>The reply as a string</returns>
    protected virtual async Task<string> SendDocumentAndAddToOutputsAsync(MessageWaiter waiter, InputFile input, CommandOutput display)
    {
        Outputs.Add(display);
        var reply = await waiter.SendDocumentAndAwaitNext(input);
        AddReplyToOutputs(reply);
        return reply.GetMessageString();
    }
}

/// <summary>
/// A record representation of a "message" that shows up on the outputs screen
/// </summary>
/// <param name="Content">String content of the message. Make null if not applicable</param>
/// <param name="ImgPath">The path to the image to be displayed. Make null if not applicable</param>
/// <param name="Right">Whether or not the image displays on the right of the display</param>
public record CommandOutput(string Content, string ImgPath, bool Right)
{
    public HorizontalAlignment Align => Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public bool HasImg => !string.IsNullOrEmpty(ImgPath);
    public bool HasText => !string.IsNullOrEmpty(Content);
    public string EnsuredImg => HasImg ? ImgPath : " ";
}
