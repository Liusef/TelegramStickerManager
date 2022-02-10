using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TgApi.Types;

namespace ReunionApp.Runners;

public abstract class CommandRunner
{
    protected StickerPack pack;
    public ObservableCollection<CommandOutput> Outputs { get; set; } = new System.Collections.ObjectModel.ObservableCollection<CommandOutput>();

    public abstract Task RunCommandsAsync();

    public virtual async Task PostTasks()
    {
        //pack.IsCachedCopy = true;
        await App.GetInstance().ResetTdClient();
        App.GetInstance().ResetFrameCache();
    }
}

public record CommandOutput(string Content, bool Right)
{
    public HorizontalAlignment Align => Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
}
