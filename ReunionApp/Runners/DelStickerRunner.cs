using System;
using System.Collections.Generic;
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
        foreach (var s in stickers)
        {
            Outputs.Add(new CommandOutput("/delsticker", true));
            var reply = await waiter.SendMsgAndAwaitNext("/delsticker");
            Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
            Outputs.Add(new CommandOutput(s.RemoteFileId, true));
            var msg = await client.SendBasicDocumentAsync(botId, new InputFileRemote { Id = s.RemoteFileId });
            reply = await waiter.WaitNextMsgAsync(msg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
        }
    }
}
