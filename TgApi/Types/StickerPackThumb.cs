using System.Text.Json.Serialization;
using TdLib;
using TgApi.Telegram;

namespace TgApi.Types;

public class StickerPackThumb
{
	/// <summary>
	/// The height of the thumb in pixels
	/// </summary>
	public int Width { get; set; }
	/// <summary>
	/// The height of the thumb in pixels
	/// </summary>
	public int Height { get; set; }
	/// <summary>
	/// The Remote File ID of the thumbnail document. This shouldn't be used.
	/// </summary>
	public string RemoteFileId { get; set; }
	/// <summary>
	/// The filename of the thumbnail document
	/// </summary>
	public string Filename { get; set; }
	/// <summary>
	/// The size of the file
	/// </summary>
	public int Size { get; set; }
	/// <summary>
	/// The type of the thumbnail
	/// </summary>
	public StickerType Type { get; set; }
	/// <summary>
	/// Whether or not the thumbnail is a dedicated thumbnail assigned to the pack, or just the first item in the pack.
	/// This is required for determining where TDLib caches the image.
	/// </summary>
	public bool IsDesignatedThumb { get; set; }

	/// <summary>
	/// The local path of the thumb on the system
	/// </summary>
	[JsonIgnore]
	public virtual string LocalPath => (IsDesignatedThumb ? GlobalVars.ThumbsDir : GlobalVars.StickersDir) + Filename;

	/// <summary>
	/// Whether or not the thumb is downloaded to the system
	/// </summary>
	[JsonIgnore]
	public bool IsDownloaded => File.Exists(LocalPath);

	/// <summary>
	/// Returns the path of the best image to use. Checks the decoded images folder.
	/// </summary>
	[JsonIgnore]
	public virtual string BestPath
	{
		get
		{
			if (IsDesignatedThumb) return LocalPath;
            var decPath = $"{GlobalVars.DecodedDir}{Path.GetFileNameWithoutExtension(Filename)}.png";
			return File.Exists(decPath) ? decPath : LocalPath;
		}
	}

	/// <summary>
	/// Generates a StickerPackThumb object from a TdApi.Thumbnail object from telegram
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="input">The TdApi.Thumbnail object from telegram</param>
	/// <param name="isAnimated">Whether or not the thumbnail is animated using TGS</param>
	/// <returns>A StickerPackThumb object derived from a TdApi.Thumbnail object</returns>
	public static async Task<StickerPackThumb?> Generate(TdClient client, TdApi.Thumbnail? input, TdApi.StickerType type)
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
			Filename = (await filenameTask).Text_
		};
		t.Type = Sticker.GetStickerType(type);
		return t;
	}

	/// <summary>
	/// Generates a StickerPackThumb object from a Sticker object. This is used when there is no dedicated thumb object assigned to the pack.
	/// </summary>
	/// <param name="input">A Sticker</param>
	/// <returns>A StickerPackThumb object derived from the Sticker</returns>
	public static StickerPackThumb FromSticker(Sticker input)
	{
		return new StickerPackThumb
		{
			Width = input.Width,
			Height = input.Height,
			RemoteFileId = input.RemoteFileId,
			Filename = input.Filename,
			Size = input.Size,
			Type = input.Type,
			IsDesignatedThumb = false
		};
	}

	/// <summary>
	/// Initiates the download of the thumbnail image from Telegram.
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <returns>A FileDownload object that tracks the download of the Image</returns>
	public async Task<FileDownload> StartDownload(TdClient client, int priority = 1) =>
		await FileDownload.StartDownload(client, (await client.GetRemoteFileAsync(RemoteFileId)).Id, priority);

	/// <summary>
	/// Initiates and ensures completion of the the thumbnail image download
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <param name="delay">The delay between polls to check if the download is complete</param>
	/// <returns>A FileDownload object that contains the download details</returns>
	public async Task<FileDownload> CompleteDownload(TdClient client, int priority = 1, int delay = 25)
	{
		var fd = await StartDownload(client, priority);
		return await fd.WaitForCompletion(delay);
	}

	/// <summary>
	/// Initiates and ensures completion of the thumbnail image download to the filename specified in the object definition
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <param name="delay">The delay between polls to check if the download is complete</param>
	/// <returns>The local path of the image</returns>
	public async Task<string> CompleteDownloadEnsureCorrectFilename(TdClient client, int priority = 1, int delay = 25)
	{
		var fd = await CompleteDownload(client, priority, delay);
		Console.WriteLine("Thumb downloaded to:" + fd.LocalPath);
		if (Filename.Equals(Path.GetFileName(fd.LocalPath))) return LocalPath;
		var desired = $"{Path.GetDirectoryName(fd.LocalPath)}{Path.DirectorySeparatorChar}{Filename}";
		File.Move(fd.LocalPath, desired);
		return desired;
	}

	/// <summary>
	/// Ensures that the thumbnail image is downloaded to the system
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <param name="delay">The delay between polls to check if the download is complete</param>
	/// <returns>The local path of the image</returns>
	public async Task<string> GetPathEnsureDownloaded(TdClient client, int priority = 1, int delay = 25)
	{
		Console.WriteLine($"Checking thumb for {LocalPath}, is Downloaded {IsDownloaded}");
		if (IsDownloaded) return LocalPath;
		return await CompleteDownloadEnsureCorrectFilename(client, priority, delay);
	}
}
