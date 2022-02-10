using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReunionApp.Pages.CommandPages;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class AddStickerRunner : CommandRunner
{
    private NewSticker[] stickers;

    public AddStickerRunner(StickerPack spack, NewSticker[] nsArr) : base()
    {
        pack = spack;
        stickers = nsArr;
    }

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);
        await waiter.SendMsgAndAwaitNext("/cancel");
        Outputs.Add(new CommandOutput("/addsticker", true));
        var reply = await waiter.SendMsgAndAwaitNext("/addsticker");
        Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
        Outputs.Add(new CommandOutput(pack.Name, true));
        reply = await waiter.SendMsgAndAwaitNext(pack.Name);
        Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
        foreach (var sticker in stickers) // TODO see if starting all the uploads first is faster
        {
            var upload = await FileUpload.StartUpload(client, sticker.EnsuredPath); 
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(TgApi.Utils.GetPathFilename(sticker.EnsuredPath), true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
            if (reply.GetMessageString().Substring(0, 7) == "Thanks!")
            {
                Outputs.Add(new CommandOutput(sticker.Emojis, true));
                reply = await waiter.SendMsgAndAwaitNext(sticker.Emojis);
                Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
            } else
            {
                continue;
            }
        }
        Outputs.Add(new CommandOutput("/done", true));
        reply = await waiter.SendMsgAndAwaitNext("/done");
        Outputs.Add(new CommandOutput(reply.GetMessageString(), false));
    }
}
