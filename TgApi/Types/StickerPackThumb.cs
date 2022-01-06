using System.Text.Json.Serialization;
using TdLib;

namespace TgApi.Types;

public class StickerPackThumb
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string RemoteFileId { get; set; }
    public string Filename { get; set; }
    public int Size { get; set; }
    public bool IsAnimated { get; set; }
    public bool IsDesignatedThumb { get; set; }


    [JsonIgnore]
    private string LocalPath => $"{GlobalVars.TdDir}" +
                                $@"{(IsDesignatedThumb ? "thumbnails" : "stickers")}" +
                                $"{Path.DirectorySeparatorChar}" +
                                $"{Filename}";
    
    [JsonIgnore]
    public bool IsDownloaded => File.Exists(LocalPath);

    public static async Task<StickerPackThumb?> Generate(TdClient client, TdApi.Thumbnail? input, bool isAnimated)
    {
        if (input is null) return null;
        var filenameTask = client.GetSuggestedFileNameAsync(input.File.Id);
        var t = new StickerPackThumb
        {
            Width = input.Width,
            Height = input.Height,
            RemoteFileId = input.File.Remote.Id,
            Size = input.File.ExpectedSize,
            IsDesignatedThumb = true,
            IsAnimated = isAnimated
        };
        t.Filename = (await filenameTask).Text_;
        return t;
    }

    public static StickerPackThumb FromSticker(Sticker input)
    {
        return new StickerPackThumb
        {
            Width = input.Width,
            Height = input.Height,
            RemoteFileId = input.RemoteFileId,
            Filename = input.Filename,
            Size = input.Size,
            IsAnimated = input.IsAnimated,
            IsDesignatedThumb = false
        };
    }
    
    public async Task<FileDownload> StartDownload(TdClient client, int priority = 1) =>
        await FileDownload.StartDownload(client, (await client.GetRemoteFileAsync(RemoteFileId)).Id, priority);

    public async Task<FileDownload> CompleteDownload(TdClient client, int priority = 1, int delay = 25)
    {
        var fd = await StartDownload(client, priority);
        return await fd.WaitForCompletion(delay);
    }
    
    public async Task<string> CompleteDownloadEnsureCorrectFilename(TdClient client, int priority = 1, int delay = 25)
    {
        var fd = await CompleteDownload(client, priority, delay);
        Console.WriteLine("Thumb downloaded to:"+fd.LocalPath);
        if (Filename.Equals(Utils.GetPathFilename(fd.LocalPath))) return LocalPath;
        var desired = $"{Utils.GetPathDirectory(fd.LocalPath)}{Filename}";
        File.Move(fd.LocalPath, desired);
        return desired;
    }
    
    public async Task<string> GetPathEnsureDownloaded(TdClient client, int priority = 1, int delay = 25)
    {
        Console.WriteLine($"Checking thumb for {LocalPath}, is Downloaded {IsDownloaded}");
        if (IsDownloaded) return LocalPath;
        return (await CompleteDownloadEnsureCorrectFilename(client, priority, delay));
    }
}
