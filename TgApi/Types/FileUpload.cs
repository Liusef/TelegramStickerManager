using System.Text.Json;
using TdLib;

namespace TgApi.Types;

public class FileUpload
{
    private TdApi.Client client;
    private TdApi.File latestFile;
    private EventHandler<TdApi.Update> handler;
    
    public double Progress => (latestFile.Remote.UploadedSize + 0.0) / latestFile.ExpectedSize;
    public int LocalId => latestFile.Id;
    public string RemoteId => latestFile.Remote.Id;
    public bool IsComplete => latestFile.Remote is not null && latestFile.Remote.IsUploadingCompleted;

    public static async Task<FileUpload> Upload(TdClient client, string path, int priority = 1, TdApi.FileType? type = null)
    {
        if (type is null) type = new TdApi.FileType.FileTypeDocument();
        
        FileUpload r = new FileUpload();
        r.latestFile = await client.UploadFileAsync(new TdApi.InputFile.InputFileLocal {Path = path}, type, priority);

        r.handler = (sender, update) =>
        {
            if (update is TdApi.Update.UpdateFile fileUpdate && fileUpdate.File.Id == r.LocalId)
            {
                r.latestFile = fileUpdate.File;
                if (r.latestFile.Remote.IsUploadingCompleted) client.UpdateReceived -= r.handler;
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
