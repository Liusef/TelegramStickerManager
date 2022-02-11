using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class DelStickerRunner : CommandRunner
{
    private Sticker[] stickers;
    private int _index;

    public override event PropertyChangedEventHandler PropertyChanged;

    private int Index 
    { 
        get => _index;
        set 
        {
            _index = value;
            OnProgressChanged();
        } 
    }

    public override double Progress => 100 * (double)Index / stickers.Length;

    protected override void OnProgressChanged() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
    

    public DelStickerRunner(Sticker[] s) : base()
    {
        stickers = s;
    }

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);
        await waiter.SendMsgAndAwaitNext("/cancel");
        for (Index = 0; Index < stickers.Length; Index++)
        {
            Outputs.Add(new CommandOutput("/delsticker", null, true));
            var reply = await waiter.SendMsgAndAwaitNext("/delsticker");
            Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
            Outputs.Add(new CommandOutput(null, stickers[Index].DecodedPath,true));
            var msg = await client.SendBasicDocumentAsync(botId, new InputFileRemote { Id = stickers[Index].RemoteFileId });
            reply = await waiter.WaitNextMsgAsync(msg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
        }
    }
}
