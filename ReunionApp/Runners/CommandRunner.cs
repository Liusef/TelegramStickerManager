﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TgApi.Telegram;
using TgApi.Types;
using static TdLib.TdApi;

namespace ReunionApp.Runners;

public abstract class CommandRunner : INotifyPropertyChanged
{
    protected StickerPack pack;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CommandOutput> Outputs { get; set; } = new System.Collections.ObjectModel.ObservableCollection<CommandOutput>();

    private double _progress = 0;
    public double Progress
    {
        get { return _progress; }
        set 
        { 
            _progress = value;
            OnProgressChanged();
        }
    }
    protected void OnProgressChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
    }

    public abstract Task RunCommandsAsync();

    public virtual async Task PreTasksAsync() 
    {
        //    var chat = await App.GetInstance().Client.GetIdFromUsernameAsync("Stickers");
        //    await App.GetInstance().Client.OpenChatAsync(chat);
        //    await App.GetInstance().Client.SetChatNotificationSettingsAsync(chat, new ChatNotificationSettings {MuteFor = int.MaxValue, UseDefaultMuteFor = false });
    }

    public virtual async Task PostTasksAsync()
    {
        var client = App.GetInstance().Client;
        var chat = await client.GetIdFromUsernameAsync("Stickers");
        await client.MarkChatAsRead(chat);
        if (pack != null) pack.IsCachedCopy = true;
    }

    public virtual void AddReplyToOutputs(Message msg) =>
        Outputs.Add(new CommandOutput(msg.GetMessageString(), null, false));

    public async virtual Task SendAndAddToOutputsAsync(MessageWaiter waiter, string message)
    {
        Outputs.Add(new CommandOutput(message, null, true));
        AddReplyToOutputs(await waiter.SendMsgAndAwaitNext(message));
    }

}

public record CommandOutput(string Content, string ImgPath, bool Right)
{
    public HorizontalAlignment Align => Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public bool HasImg => !string.IsNullOrEmpty(ImgPath);
    public string EnsuredImg => HasImg ? ImgPath : " ";
    public bool HasText => !string.IsNullOrEmpty(Content);
}