using SixLabors.ImageSharp;
using System.Text.Json.Serialization;
using TdLib;

namespace TgApi.Types;

public class StickerPack
{
	/// <summary>
	/// The ID of this sticker pack
	/// </summary>
	public long Id { get; set; }
	/// <summary>
	/// The title of this sticker pack
	/// </summary>
	public string Title { get; set; }
	/// <summary>
	/// The name of this sticker pack. This is what's used in the URL
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// The type of the sticker pack
	/// </summary>
	public StickerType Type { get; set; }
	/// <summary>
	/// A StickerPackThumb object. This is populated when the pack has a designated thumbnail
	/// </summary>
	public StickerPackThumb? Thumb { get; set; }

	/// <summary>
	/// An array containing all the stickers in the pack
	/// </summary>
	[JsonIgnore]
	public Sticker[] Stickers { get; set; }

	/// <summary>
	/// Whether or not this is a cached copy of the sticker pack.
	/// Cached copies do not have stickers within them
	/// </summary>
	public bool IsCachedCopy = false;

	/// <summary>
	/// How many stickers are in the pack. This is only not 0 when it is not a cached copy
	/// </summary>
	[JsonIgnore]
	public int Count => Stickers.Length;

	/// <summary>
	/// The uri to add the sticker through your browser
	/// </summary>
	[JsonIgnore] public Uri AddStickerUri => new Uri($"https://t.me/addstickers/{Name}");

	/// <summary>
	/// Always returns a thumbnail. If the pack has no designated thumb it returns the first sticker in the pack.
	/// </summary>
	public StickerPackThumb EnsuredThumb
	{
		get => Thumb ?? StickerPackThumb.FromSticker(Stickers[0]);
		set => Thumb = value;
	}

	/// <summary>
	/// Generates a StickerPack object from a TdApi.StickerSet object from Telegram
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="input">A TdApi.StickerSet object from Telegram</param>
	/// <returns></returns>
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

	/// <summary>
	/// Gets a StickerPack object based on the name of the sticker pack
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="name">The name of the sticker pack</param>
	/// <returns></returns>
	public static async Task<StickerPack> GenerateFromName(TdClient client, string name) =>
		await Generate(client, await client.SearchStickerSetAsync(name));

	/// <summary>
	/// Initiates a download for all stickers in the pack
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The priority of the download from 1 to 32</param>
	/// <returns>An array of FileDownload objects</returns>
	public async Task<FileDownload[]> StartDownloadAll(TdClient client, int priority = 1)
	{
		var fdList = new List<FileDownload>();
		foreach (var s in Stickers)
		{
			fdList.Add(await s.StartDownload(client, priority));
		}
		return fdList.ToArray();
	}

	/// <summary>
	/// Initiates an ensured download completion for all stickers in the pack
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The priority of the download from 1 to 32</param>
	/// <returns>An array of FileDownload objects</returns>
	public async Task<FileDownload[]> CompleteDownloadAll(TdClient client, int priority = 1, int delay = 25)
	{
		var fdList = new List<FileDownload>();
		foreach (var fd in await StartDownloadAll(client, priority))
		{
			fdList.Add(await fd.WaitForCompletion(delay));
		}
		return fdList.ToArray();
	}

	/// <summary>
	/// Ensures all stickers are downloaded to the system
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The priority of the download from 1 to 32</param>
	/// <param name="delay">The delay between polling to check if downloaded</param>
	/// <returns>An array of paths</returns>
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

	/// <summary>
	/// Ensures all stickers are downloaded and decoded on the system
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The priority of the download from 1 to 32</param>
	/// <param name="delay">The delay between polling to check if downloaded</param>
	/// <returns>The local paths of all decoded stickers</returns>
	public async Task<string[]> EnsureAllDecodedDownloaded(TdClient client, int priority = 1, int delay = 25)
	{
		var r = new List<string>();
		foreach (var s in Stickers)
		{
			r.Add(await s.GetDecodedPathEnsureDownloaded(client, priority, delay));
		}
		return r.ToArray();
	}

	/// <summary>
	/// Ensures all stickers are downloaded and decoded on the system using parallelization
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="threads">The amount of threads to use for decoding</param>
	/// <param name="priority">The priority of the download from 1 to 32</param>
	/// <param name="delay">The delay between polling to check if downloaded</param>
	/// <returns>The local paths of all decoded stickers</returns>
	public async Task<string[]> EnsureAllDecodedDownloadedParallel(TdClient client, int threads, int priority = 1, int delay = 25)
	{
		await EnsureAllDownloaded(client, priority, delay);
		await Parallel.ForEachAsync(Stickers, new ParallelOptions { MaxDegreeOfParallelism = threads }, async (sticker, ct) =>
		{
			if (!sticker.DecodedCopySaved) await sticker.DecodeSticker();
			ct.ThrowIfCancellationRequested();
		});
		Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
		//GC.Collect();
		return Stickers.Select(x => x.DecodedPath).ToArray();
	}

	/// <summary>
	/// Caches the pack to the system
	/// </summary>
	public void Cache() => Utils.Serialize<StickerPack>(this, $"{GlobalVars.PacksDir}{Name}.json");

	/// <summary>
	/// Reads a pack from the system's memory. This method does not check if the pack is present
	/// </summary>
	/// <param name="name">The name of the pack</param>
	/// <returns></returns>
	public static StickerPack ReadCache(string name)
	{
		var pack = Utils.Deserialize<StickerPack>($"{GlobalVars.PacksDir}{name}.json");
		pack.IsCachedCopy = true;
		return pack;
	}

	/// <summary>
	/// Gets a pack that isn't guaranteed to have all stickers in it
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="name">The name of the pack </param>
	/// <returns>A StickerPack object</returns>
	public static async Task<StickerPack> GetBasicPack(TdClient client, string name)
	{
		if (File.Exists($"{GlobalVars.PacksDir}{name}.json")) return ReadCache(name);
		return await GenerateFromName(client, name);
	}

	/// <summary>
	/// Adds complete information from fully detailed stickerpack to basic version
	/// </summary>
	/// <param name="full"></param>
	public void InjectCompleteInfo(StickerPack full)
	{
		if (!IsCachedCopy) return;
		Stickers = full.Stickers;
		if (Thumb == null || !Thumb.IsDesignatedThumb || (full.Thumb != null && full.Thumb.RemoteFileId != Thumb.RemoteFileId))
		{
			if (full.Thumb != null && full.Thumb.IsDesignatedThumb) Thumb = full.Thumb;
		}
		IsCachedCopy = false;
	}
}
