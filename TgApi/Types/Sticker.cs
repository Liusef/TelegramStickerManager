using System.Text.Json.Serialization;
using TdLib;

namespace TgApi.Types;

public class Sticker
{
    public long ParentId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string MainEmoji { get; set; }
    public string Emojis { get; set; }
    public StickerType Type { get; set; }
    public string Filename { get; set; }
    public string RemoteFileId { get; set; }
    public int Size { get; set; }

    [JsonIgnore]
    private string LocalPath => $"{GlobalVars.TdDir}stickers{Path.DirectorySeparatorChar}{Filename}";
    
    [JsonIgnore]
    public bool IsDownloaded => File.Exists(LocalPath);

    public Sticker() { }

    public static async Task<Sticker> Generate(TdClient client, TdApi.Sticker input)
    {
        var filenameTask = client.GetSuggestedFileNameAsync(input.Sticker_.Id);
        var inputEmojisTask = client.GetStickerEmojisAsync(new TdApi.InputFile.InputFileId {Id = input.Sticker_.Id});
        var s = new Sticker
        {
            ParentId = input.SetId,
            Width = input.Width,
            Height = input.Height,
            RemoteFileId = input.Sticker_.Remote.Id,
            Size = input.Sticker_.ExpectedSize,
            MainEmoji = input.Emoji
        };
        TdApi.Emojis inputEmojis = await inputEmojisTask;
        List<string> emojis = new List<string>(inputEmojis.Emojis_);
        emojis.Remove(input.Emoji);
        s.Emojis = input.Emoji + string.Join("", emojis);
        s.Filename = (await filenameTask).Text_;

        if (input.IsMask) s.Type = StickerType.MASK;
        else if (input.IsAnimated) s.Type = StickerType.ANIMATED;
        else if (s.Filename.Substring(s.Filename.Length - 4).Equals("webm")) s.Type = StickerType.VIDEO;
        else s.Type = StickerType.STANDARD;

        return s;
    }

    public async Task<FileDownload> StartDownload(TdClient client, int priority = 1) =>
        await FileDownload.StartDownload(client, (await client.GetRemoteFileAsync(RemoteFileId)).Id, priority);

    public async Task<FileDownload> CompleteDownload(TdClient client, int priority = 1, int delay = 25)
    {
        var fd = await StartDownload(client, priority);
        return await fd.WaitForCompletion(delay);
    }

    public async Task<string> GetPathEnsureDownloaded(TdClient client, int priority = 1, int delay = 25)
    {
        if (IsDownloaded) return LocalPath;
        return (await CompleteDownload(client, priority, delay)).LocalPath;
    }

}
