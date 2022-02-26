using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using ReunionApp.Pages.CommandPages;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class NewPackRunner : CommandRunner
{
    private new StickerPack pack;
    private NewSticker[] newStickers;
    private int _index;
    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            Progress = 100 * (double)Index / newStickers.Length;
        }
    }

    public NewPackRunner(StickerPack p, NewSticker[] ns)
    {
        pack = p;
        newStickers = ns;
    }

    private async Task AskForNewName()
    {
        var cd = new ContentDialog
        {
            Title = "The provided name was invalid",
            IsPrimaryButtonEnabled = false,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Enter"
        };
        var b = new RunnerDependencies.NewPackRunnerNameDialog { Dialog = cd, Pack = pack };
        cd.Content = b;
        cd.XamlRoot = App.GetInstance().MainWindow.Content.XamlRoot;

        cd.Closing += (sender, args) =>
        {
            if (!b.IsValid()) args.Cancel = true;
            pack.Name = b.UserInput.Text;
        };

        await cd.ShowAsync();
    }

    public override async Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);
        await waiter.SendMsgAndAwaitNext("/cancel");

        await SendAndAddToOutputsAsync(waiter, "/newpack");
        await SendAndAddToOutputsAsync(waiter, pack.Title);

        for (Index = 0; Index < newStickers.Length; Index++)
        {
            var upload = await FileUpload.StartUpload(client, newStickers[Index].EnsuredPath);
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(null, newStickers[Index].ImgPath, true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            var reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            AddReplyToOutputs(reply);

            if (reply.GetMessageString()[..7] == "Thanks!") await SendAndAddToOutputsAsync(waiter, newStickers[Index].Emojis);
        }

        await SendAndAddToOutputsAsync(waiter, "/publish");

        await App.GetInstance().ShowBasicDialog("Path", $"LocalPath: {pack.Thumb.LocalPath}, {File.Exists(pack.Thumb.LocalPath)}\nBestPath: {pack.Thumb.BestPath}, {File.Exists(pack.Thumb.BestPath)}");

        if (File.Exists(pack.Thumb.LocalPath))
        {
            var upload = await FileUpload.StartUpload(client, pack.Thumb.LocalPath);
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(null, pack.Thumb.LocalPath, true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            var reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            AddReplyToOutputs(reply);
        }
        else await SendAndAddToOutputsAsync(waiter, "/skip");

        Outputs.Add(new CommandOutput(pack.Name, null, true));
        var r = await waiter.SendMsgAndAwaitNext(pack.Name);
        AddReplyToOutputs(r);

        while (r.GetMessageString()[..5] == "Sorry")
        {
            await AskForNewName();
            Outputs.Add(new CommandOutput(pack.Name, null, true));
            r = await waiter.SendMsgAndAwaitNext(pack.Name);
            AddReplyToOutputs(r);
        }
    }
}
