using System.Threading.Tasks;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class DelStickerRunner : CommandRunner
{
    private Sticker[] stickers;
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

    public DelStickerRunner(Sticker[] s) => stickers = s;

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);

        await waiter.SendMsgAndAwaitNext("/cancel");

        for (Index = 0; Index < stickers.Length; Index++)
        {
            await SendAndAddToOutputsAsync(waiter, "/delsticker");
            await SendDocumentAndAddToOutputsAsync(waiter, new InputFileRemote { Id = stickers[Index].RemoteFileId },
                new CommandOutput(null, stickers[Index].BestPath, true));
        }
    }
}
