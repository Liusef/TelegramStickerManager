using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    protected override void OnProgressChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
    }

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
        Outputs.Add(new CommandOutput("/addsticker", null, true));
        var reply = await waiter.SendMsgAndAwaitNext("/addsticker");
        Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
        Outputs.Add(new CommandOutput(pack.Name, null, true));
        reply = await waiter.SendMsgAndAwaitNext(pack.Name);
        Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
        for (Index = 0; Index < stickers.Length; Index++) // TODO see if starting all the uploads first is faster
        {
            var upload = await FileUpload.StartUpload(client, stickers[Index].EnsuredPath); 
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(null, stickers[Index].ImgPath, true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
            if (reply.GetMessageString().Substring(0, 7) == "Thanks!")
            {
                Outputs.Add(new CommandOutput(stickers[Index].Emojis, null, true));
                reply = await waiter.SendMsgAndAwaitNext(stickers[Index].Emojis);
                Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
            } else
            {
                continue;
            }
        }
        Outputs.Add(new CommandOutput("/done", null, true));
        reply = await waiter.SendMsgAndAwaitNext("/done");
        Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
    }
}
