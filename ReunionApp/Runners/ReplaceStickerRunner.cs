using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReunionApp.Pages.CommandPages;
using TgApi.Telegram;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;
public class ReplaceStickerRunner : CommandRunner
{
    private ReplaceStickerUpdate[] updates;
    private int _index;
    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            Progress = 100 * (_index + 0.0) / updates.Length;
        }
    }

    public ReplaceStickerRunner(ReplaceStickerUpdate[] updates) => this.updates = updates;

    public async override Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);

        await waiter.SendMsgAndAwaitNext("/cancel");
        await SendAndAddToOutputsAsync(waiter, "/replacesticker");

        for (Index = 0; Index < updates.Length; Index++)
        {
            if (!File.Exists(updates[Index].NewPath)) continue;

            var r = await SendDocumentAndAddToOutputsAsync(waiter, new InputFileRemote { Id = updates[Index].Sticker.RemoteFileId },
                new CommandOutput(null, updates[Index].Sticker.BestPath, true));

            if (r[..7] != "Alright") continue;

            r = await SendImageAndAddToOutputsAsync(waiter, updates[Index].NewPath);

            if (r[..10] != "I replaced")
            {
                await SendAndAddToOutputsAsync(waiter, "/cancel");
                await SendAndAddToOutputsAsync(waiter, "/replacesticker");
            }
        }
        await SendAndAddToOutputsAsync(waiter, "/done");
        await SendAndAddToOutputsAsync(waiter, "/cancel");
    }
}
