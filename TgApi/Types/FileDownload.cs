using System.Text.Json;
using TdLib;

namespace TgApi.Types;

public class FileDownload
{
    private double progress;
    private int localId;
    private TdApi.Client client;
    private TdApi.File latestFile;
    private EventHandler<TdApi.Update> handler;

    public double Progress { get => progress; }
    public int LocalId { get => localId; }

    public bool IsComplete
    {
        get
        {
            if (latestFile.Local is null) return false;
            return latestFile.Local.IsDownloadingCompleted;
        }
    }

    public static async Task<FileDownload> Download(TdClient client, int id, int priority = 1)
    {
        FileDownload r = new FileDownload();
        r.client = client;
        r.localId = id;
        r.progress = 0;

        r.latestFile = await client.DownloadFileAsync(fileId: id, priority:priority);

        r.handler = (object? sender, TdApi.Update update) =>
        {
            if (update is TdApi.Update.UpdateFile && ((TdApi.Update.UpdateFile)update).File.Id == r.localId)
            {
                var file = ((TdApi.Update.UpdateFile)update).File;
                r.progress = (file.Local.DownloadedSize + 0.0) / file.ExpectedSize;
                r.latestFile = file;
                if (file.Local.IsDownloadingCompleted)
                {
                    client.UpdateReceived -= r.handler;
                }
            }
        };

        client.UpdateReceived += r.handler;
        return r;
    }

    public async Task WaitForCompletion(int delay = 25)
    {
        while (latestFile.Local is null || !latestFile.Local.IsDownloadingCompleted) await Task.Delay(delay);
    }
    
}
