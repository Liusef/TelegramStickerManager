using System.Text.Json.Serialization;
using TdLib;

namespace TgApi.Types;

public class StickerPack
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Name { get; set; }
    public StickerType Type { get; set; }
    public StickerPackThumb? Thumb { get; set; }

    [JsonIgnore]
    public Sticker[] Stickers { get; set; }

    public bool IsCachedCopy = false;

    [JsonIgnore]
    public int Count => Stickers.Length;

    public StickerPackThumb EnsuredThumb 
    { 
        get => Thumb is null ? StickerPackThumb.FromSticker(Stickers[0]) : Thumb; 
        set => Thumb = value;
    }

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
            Thumb = await StickerPackThumb.Generate(client, input.Thumbnail, input.IsAnimated),
        };
        var slist = new List<Sticker>();
        foreach (var task in taskList)
        {
            slist.Add(await task);
        }
        s.Stickers = slist.ToArray();

        if (input.IsMasks) s.Type = StickerType.MASK;
        else if (input.IsAnimated) s.Type = StickerType.ANIMATED;
        else if (s.Stickers[0].Filename.Substring(s.Stickers[0].Filename.Length - 4).Equals("webm")) s.Type = StickerType.VIDEO;
        else s.Type = StickerType.STANDARD;

        s.Cache();
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

    public void Cache() => Utils.Serialize<StickerPack>(this, $"{GlobalVars.PacksDir}{Name}.json");

    public static StickerPack ReadCache(string name)
    {
        var pack = Utils.Deserialize<StickerPack>($"{GlobalVars.PacksDir}{name}.json");
        pack.IsCachedCopy = true;
        return pack;
    }

    public static async Task<StickerPack> GetBasicPack(TdClient client, string name)
    {
        if (File.Exists($"{GlobalVars.PacksDir}{name}.json")) return ReadCache(name);
        return await GenerateFromName(client, name);
    }
}
