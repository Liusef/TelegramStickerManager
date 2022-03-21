using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReunionApp.Pages.CommandPages;
using TgApi.Telegram;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;
internal class EditStickerRunner : CommandRunner
{
    private EditStickerUpdate[] updates;

    private int _index = 0;
    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            Progress = 100 * (value + 0.0)  / updates.Length;
        }
    }

    public EditStickerRunner(EditStickerUpdate[] updates) : base() => this.updates = updates;

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);

        await waiter.SendMsgAndAwaitNext("/cancel");

        for (Index = 0; Index < updates.Length; Index++)
        {
            await SendAndAddToOutputsAsync(waiter, "/editsticker");
            await SendDocumentAndAddToOutputsAsync(waiter, new InputFileRemote { Id = updates[Index].sticker.RemoteFileId },
                new CommandOutput(null, updates[Index].sticker.BestPath, true));
            await SendAndAddToOutputsAsync(waiter, updates[Index].newEmoji);
        }
    }
}
