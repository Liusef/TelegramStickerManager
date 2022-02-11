using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TgApi.Types;

namespace ReunionApp.Runners;

public abstract class CommandRunner : INotifyPropertyChanged
{
    protected StickerPack pack;

    public abstract event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<CommandOutput> Outputs { get; set; } = new System.Collections.ObjectModel.ObservableCollection<CommandOutput>();

    public abstract double Progress { get; }

    protected abstract void OnProgressChanged();

    public abstract Task RunCommandsAsync();

    public virtual async Task PostTasks()
    {
        if (pack != null) pack.IsCachedCopy = true;
        await App.GetInstance().ResetTdClient();
        App.GetInstance().ResetFrameCache();
    }
}

public record CommandOutput(string Content, string ImgPath, bool Right)
{
    public HorizontalAlignment Align => Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    public bool HasImg => !string.IsNullOrEmpty(ImgPath);
    public string EnsuredImg => HasImg ? ImgPath : " ";
    public bool HasText => !string.IsNullOrEmpty(Content);
}
