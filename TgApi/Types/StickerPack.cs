using System.Text.Json.Serialization;
using TdLib;

namespace TgApi.Types;

public class StickerPack : BasicPack
{
    public Sticker[] Stickers { get; set; }

    [JsonIgnore]
    public int Count => Stickers.Length;

    [JsonIgnore]
    public StickerPackThumb EnsuredThumb => Thumb is null ? StickerPackThumb.FromSticker(Stickers[0]) : Thumb;

    public static async Task<StickerPack> Generate(TdClient client, TdApi.StickerSet input)
    {
        var taskList = new List<Task<Sticker>>();
        foreach (var sticker in input.Stickers)
        {
            taskList.Add(Sticker.Generate(client, sticker));
        }
        var s = new StickerPack
        {
            Id = input.Id,
            Title = input.Title,
            Name = input.Name,
            IsAnimated = input.IsAnimated,
            Thumb = await StickerPackThumb.Generate(client, input.Thumbnail, input.IsAnimated),
        };
        var slist = new List<Sticker>();
        foreach (var task in taskList)
        {
            slist.Add(await task);
        }
        s.Stickers = slist.ToArray();
        return s;
    }

    public static async Task<StickerPack> GenerateFromName(TdClient client, string name) =>
        await Generate(client, await client.SearchStickerSetAsync(name));

    public async Task<FileDownload[]> StartDownloadAll(TdClient client, int priority = 1)
    {
        var fdList = new List<FileDownload>();
        foreach (var s in Stickers)
        {
            fdList.Add(await s.StartDownload(client, priority));
        }
        return fdList.ToArray();
    }

    public async Task<FileDownload[]> CompleteDownloadAll(TdClient client, int priority = 1, int delay = 25)
    {
        var fdList = new List<FileDownload>();
        foreach (var fd in await StartDownloadAll(client, priority))
        {
            fdList.Add(await fd.WaitForCompletion(delay));
        }
        return fdList.ToArray();
    }

    public async Task<string[]> EnsureAllDownloaded(TdClient client, int priority = 1, int delay = 25)
    {
        var taskList = new List<Task<string>>();
        foreach (var s in Stickers)
        {
            taskList.Add(s.GetPathEnsureDownloaded(client, priority, delay));
        }
        var rlist = new List<string>();
        foreach (var task in taskList)
        {
            rlist.Add(await task);
        }
        return rlist.ToArray();
    }
}
