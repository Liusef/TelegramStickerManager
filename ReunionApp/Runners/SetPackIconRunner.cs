using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;
public class SetPackIconRunner : CommandRunner
{

    private string path;

    public SetPackIconRunner(StickerPack pack, string path)
    {
        this.pack = pack;
        this.path = path;
    }

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("stickers");
        var waiter = new MessageWaiter(client, botId);

        await waiter.SendMsgAndAwaitNext("/cancel");

        await SendAndAddToOutputsAsync(waiter, "/setpackicon");
        await SendAndAddToOutputsAsync(waiter, pack.Name);

        if (string.IsNullOrWhiteSpace(path))
        {
            await SendAndAddToOutputsAsync(waiter, "/empty");
        } 
        else
        {
            Outputs.Add(new CommandOutput(null, path, true));
            var upload = await FileUpload.StartUpload(client, path);
            await upload.WaitForCompletion();
            var reply = await waiter.SendDocumentAndAwaitNext(new InputFileId { Id = upload.LocalId });
            AddReplyToOutputs(reply);

            if (reply.GetMessageString()[..4] != "Your")
            {
                await SendAndAddToOutputsAsync(waiter, "/cancel");
            }
        }
        Progress = 100;
    }
}
