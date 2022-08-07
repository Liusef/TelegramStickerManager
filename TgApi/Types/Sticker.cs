using System.Text.Json.Serialization;
using TdLib;
using TgApi.Telegram;

namespace TgApi.Types;

public class Sticker
{
	/// <summary>
	/// The Id of the StickerPack this Sticker belongs to
	/// </summary>
	public long ParentId { get; set; }
	/// <summary>
	/// The width of the sticker in pixels
	/// </summary>
	public int Width { get; set; }
	/// <summary>
	/// The Height of the sticker in pixels
	/// </summary>
	public int Height { get; set; }
	/// <summary>
	/// The Primary emoji associated with this sticker
	/// </summary>
	public string MainEmoji { get; set; }
	/// <summary>
	/// A string containing all emojis associated with this sticker
	/// </summary>
	public string Emojis { get; set; }
	/// <summary>
	/// The type of the sticker
	/// </summary>
	public StickerType Type { get; set; }
	/// <summary>
	/// The filename of the sticker document
	/// </summary>
	public string Filename { get; set; }
	/// <summary>
	/// The Remote File ID of the sticker document.
	/// </summary>
	public string RemoteFileId { get; set; }
	/// <summary>
	/// The size of the sticker
	/// </summary>
	public int Size { get; set; }

	/// <summary>
	/// The local path of the sticker on the system
	/// </summary>
	[JsonIgnore]
	public string LocalPath => $"{GlobalVars.StickersDir}{Filename}";

	/// <summary>
	/// Whether or not the sticker is downloaded to the system
	/// </summary>
	[JsonIgnore]
	public bool LocalCopySaved => File.Exists(LocalPath);

	/// <summary>
	/// Gets the path of the pre-decoded PNG version of the sticker
	/// </summary>
	[JsonIgnore]
	public string DecodedPath => $"{GlobalVars.DecodedDir}{Path.GetFileNameWithoutExtension(Filename)}.png";

	/// <summary>
	/// Whether or not the decoded version of the sticker is downloaded to the system
	/// </summary>
	[JsonIgnore]
	public bool DecodedCopySaved => File.Exists(DecodedPath);

	/// <summary>
	/// Gets the path of the best available local copy on the system.
	/// Assumes that at least one copy is downloaded on the system.
	/// </summary>
	[JsonIgnore]
	public string BestPath => DecodedCopySaved ? DecodedPath : LocalPath;

	/// <summary>
	/// Generates a Sticker object from a TdApi.Sticker object from telegram
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="input">The TdApi.Sticker object from Telegram</param>
	/// <returns>A Sticker object derived from a TdApi.Sticker object</returns>
	public static async Task<Sticker> Generate(TdClient client, TdApi.Sticker input)
	{
		var filenameTask = client.GetSuggestedFileNameAsync(input.Sticker_.Id);
		var inputEmojisTask = client.GetStickerEmojisAsync(new TdApi.InputFile.InputFileId { Id = input.Sticker_.Id });
		var s = new Sticker
		{
			ParentId = input.SetId,
			Width = input.Width,
			Height = input.Height,
			RemoteFileId = input.Sticker_.Remote.Id,
			Size = input.Sticker_.ExpectedSize,
			MainEmoji = input.Emoji
		};
		var inputEmojis = await inputEmojisTask;
		s.Emojis = string.Join("", inputEmojis.Emojis_);
		s.Filename = (await filenameTask).Text_;
        s.Type = GetStickerType(input.Type);
		return s;
	}

	/// <summary>
	/// Initiates the download of the sticker image from Telegram.
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <returns>A FileDownload object that tracks the download of the Image</returns>
	public async Task<FileDownload> StartDownload(TdClient client, int priority = 1) =>
		await FileDownload.StartDownload(client, (await client.GetRemoteFileAsync(RemoteFileId)).Id, priority);

	/// <summary>
	/// Initiates and ensures completion of the the sticker image download
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
	/// Ensures that the sticker image is downloaded to the system
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <param name="delay">The delay between polls to check if the download is complete</param>
	/// <returns>The local path of the image</returns>
	public async Task<string> GetPathEnsureDownloaded(TdClient client, int priority = 1, int delay = 25) =>
		LocalCopySaved ? LocalPath : (await CompleteDownload(client, priority, delay)).LocalPath;
	

	public async Task<string> DecodeSticker()
	{
		if (!LocalCopySaved) throw new FileNotFoundException("The local copy was not downloaded before attempting to decode");
		return await ImgUtils.EncodeToFormat(LocalPath, DecodedPath);
	}

	/// <summary>
	/// Ensures that the sticker image is downloaded and decoded to the proper path
	/// </summary>
	/// <param name="client">An active TdClient</param>
	/// <param name="priority">The download priority from 1 to 32</param>
	/// <param name="delay">The delay between polls to check if the download is complete</param>
	/// <returns>The local path of the decoded image</returns>
	public async Task<string> GetDecodedPathEnsureDownloaded(TdClient client, int priority = 1, int delay = 25)
	{
		if (DecodedCopySaved) return DecodedPath;
		await GetPathEnsureDownloaded(client, priority, delay);
		return await DecodeSticker();
	}

    /// <summary>
    /// Converts a StickerType object to a StickerType Enum
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static StickerType GetStickerType(TdApi.StickerType stickerType) =>
        stickerType.DataType switch
        {
            "stickerTypeAnimated"   => StickerType.Animated,
            "stickerTypeMask"       => StickerType.Mask,
            "stickerTypeStatic"     => StickerType.Standard,
            "stickerTypeVideo"      => StickerType.Video,
            _ => throw new NotImplementedException($"The application received a sticker of type {stickerType.DataType}, and idk I didn't implement it.")
        };
}
