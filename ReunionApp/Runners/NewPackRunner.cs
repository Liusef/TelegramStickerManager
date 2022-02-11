using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using ReunionApp.Pages.CommandPages;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi.InputFile;

namespace ReunionApp.Runners;

public class NewPackRunner : CommandRunner
{
    private StickerPack pack;
    private NewSticker[] newStickers;
    private int _index;
    private int Index
    {
        get => _index;
        set
        {
            _index = value;
            OnProgressChanged();
        }
    }

    public NewPackRunner(StickerPack p, NewSticker[] ns) : base()
    {
        pack = p;
        newStickers = ns;
    }

    public override double Progress => 100 * (double)Index / newStickers.Length;

    public override event PropertyChangedEventHandler PropertyChanged;

    protected override void OnProgressChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
    }

    private async Task AskForNewName()
    {
        var cd = new ContentDialog();
        cd.Title = "The provided name was invalid";
        cd.IsPrimaryButtonEnabled = false;
        cd.IsSecondaryButtonEnabled = false;
        cd.CloseButtonText = "Enter";
        var b = new RunnerDependencies.NewPackRunnerNameDialog {Dialog = cd, Pack = pack};
        cd.Content = b;
        cd.XamlRoot = App.GetInstance().MainWindow.Content.XamlRoot;

        cd.Closing += (ContentDialog sender, ContentDialogClosingEventArgs args) =>
        {
            if (!b.IsValid()) args.Cancel = true;
            pack.Name = b.UserInput.Text;
        };

        await cd.ShowAsync();
    }

    public async override Task RunCommandsAsync()
    {
        var client = App.GetInstance().Client;
        var botId = await client.GetIdFromUsernameAsync("Stickers");
        var waiter = new MessageWaiter(client, botId);
        await waiter.SendMsgAndAwaitNext("/cancel");
        Outputs.Add(new CommandOutput("/newpack", null, true));
        AddReplyToOutputs(await waiter.SendMsgAndAwaitNext("/newpack"));
        Outputs.Add(new CommandOutput(pack.Title, null, true));
        AddReplyToOutputs(await waiter.SendMsgAndAwaitNext(pack.Title));
        for (Index = 0; Index < newStickers.Length; Index++) 
        {
            var upload = await FileUpload.StartUpload(client, newStickers[Index].EnsuredPath);
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(null, newStickers[Index].ImgPath, true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            var reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
            if (reply.GetMessageString().Substring(0, 7) == "Thanks!")
            {
                Outputs.Add(new CommandOutput(newStickers[Index].Emojis, null, true));
                AddReplyToOutputs(await waiter.SendMsgAndAwaitNext(newStickers[Index].Emojis));
            }
        }
        Outputs.Add(new CommandOutput("/publish", null, true));
        AddReplyToOutputs(await waiter.SendMsgAndAwaitNext("/publish"));
        if (File.Exists(pack.Thumb.LocalPath))
        {
            var upload = await FileUpload.StartUpload(client, pack.Thumb.LocalPath);
            await upload.WaitForCompletion();
            Outputs.Add(new CommandOutput(null, pack.Thumb.LocalPath, true));
            var cmsg = await client.SendBasicDocumentAsync(botId, new InputFileId { Id = upload.LocalId }); // TODO Could use InputFileLocal instead of a FileUpload
            var reply = await waiter.WaitNextMsgAsync(cmsg.Id);
            Outputs.Add(new CommandOutput(reply.GetMessageString(), null, false));
        } 
        else
        {
            Outputs.Add(new CommandOutput("/skip", null, true));
            AddReplyToOutputs(await waiter.SendMsgAndAwaitNext("/skip"));
        }
        Outputs.Add(new CommandOutput(pack.Name, null, true));
        var r = await waiter.SendMsgAndAwaitNext(pack.Name);
        Outputs.Add(new CommandOutput(r.GetMessageString(), null, false));
        while (r.GetMessageString()[0..5] == "Sorry")
        {
            await AskForNewName();
            Outputs.Add(new CommandOutput(pack.Name, null, true));
            r = await waiter.SendMsgAndAwaitNext(pack.Name);
            Outputs.Add(new CommandOutput(r.GetMessageString(), null, false));
        }
    }
}
