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
    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            Progress = 100 * (double)Index / stickers.Length;
        }
    }

    public AddStickerRunner(StickerPack spack, NewSticker[] nsArr)
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
        await SendAndAddToOutputsAsync(waiter, "/addsticker");
        await SendAndAddToOutputsAsync(waiter, pack.Name);

        for (Index = 0; Index < stickers.Length; Index++)
        {
            var upload = await FileUpload.StartUpload(client, stickers[Index].EnsuredPath);
            Outputs.Add(new CommandOutput(null, stickers[Index].ImgPath, true) { Upload = upload });
            await upload.WaitForCompletion();
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId });
            var reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            AddReplyToOutputs(reply);

            if (reply.GetMessageString()[..7] == "Thanks!") await SendAndAddToOutputsAsync(waiter, stickers[Index].Emojis);
        }
        await SendAndAddToOutputsAsync(waiter, "/done");
    }
}
