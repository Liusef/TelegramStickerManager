using System.Threading.Tasks;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class OrderStickerRunner : CommandRunner
{
    private (Sticker, Sticker)[] swaps;
    private int _index = 0;

    public OrderStickerRunner((Sticker, Sticker)[] inputSwaps)
    {
        swaps = inputSwaps;
    }

    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            Progress = 100 * (double)Index / swaps.Length;
        }
    }

    public async override Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);
        await waiter.SendMsgAndAwaitNext("/cancel");
        await SendAndAddToOutputsAsync(waiter, "/ordersticker");
        for (Index = 0; Index < swaps.Length; Index++)
        {
            Outputs.Add(new CommandOutput(null, swaps[Index].Item1.DecodedPath, true));
            var msg = await client.SendBasicDocumentAsync(botId, new InputFileRemote { Id = swaps[Index].Item1.RemoteFileId });
            AddReplyToOutputs(await waiter.WaitNextMsgAsync(msg.Id));
            Outputs.Add(new CommandOutput(null, swaps[Index].Item2.DecodedPath, true));
            msg = await client.SendBasicDocumentAsync(botId, new InputFileRemote { Id = swaps[Index].Item2.RemoteFileId });
            AddReplyToOutputs(await waiter.WaitNextMsgAsync(msg.Id));
        }
        await SendAndAddToOutputsAsync(waiter, "/done");
    }
}
