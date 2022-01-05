using System.Text.Json;
using TdLib;

namespace TgApi.Types;

public class FileDownload
{
    private TdApi.Client client;
    private TdApi.File latestFile;
    private EventHandler<TdApi.Update> handler;

    public double Progress => (latestFile.Local.DownloadedSize + 0.0) / latestFile.ExpectedSize;
    public int LocalId => latestFile.Id;
    public string RemoteId => latestFile.Remote.Id;
    public bool IsComplete => latestFile.Local is not null && latestFile.Local.IsDownloadingCompleted;
    
    
    public static async Task<FileDownload> Download(TdClient client, int id, int priority = 1)
    {
        FileDownload r = new FileDownload();
        r.client = client;

        r.latestFile = await client.DownloadFileAsync(fileId: id, priority:priority);

        r.handler = (object? sender, TdApi.Update update) =>
        {
            if (update is TdApi.Update.UpdateFile fileUpdate && fileUpdate.File.Id == r.LocalId)
            {
                r.latestFile = fileUpdate.File;
                if (r.latestFile.Local.IsDownloadingCompleted) client.UpdateReceived -= r.handler;
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
