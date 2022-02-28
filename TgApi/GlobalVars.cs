namespace TgApi;

public static class GlobalVars
{

	private static readonly char Sep = Path.DirectorySeparatorChar;

	public const int ApiId = ApiKeys.ApiId;
	public const string ApiHash = ApiKeys.ApiHash;

	public const string ApplicationVersion = "0.2.2-alpha";
	public const string DeviceModel = "PC";
	public const string SystemLanguageCode = "en";

	public const string StickerBot = "Stickers";

	public static readonly string TdDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Sep}TgStickers{Sep}";
	public static readonly string PacksDir = $"{TdDir}documents{Sep}";
	public static readonly string StickersDir = $"{TdDir}stickers{Sep}";
	public static readonly string DecodedDir = $"{StickersDir}decoded{Sep}";
	public static readonly string ThumbsDir = $"{TdDir}thumbnails{Sep}";
	public static readonly string TempDir = $"{TdDir}temp{Sep}";

	public const string PacksFileName = "packs.json";

	/// <summary>
	/// Ensures that all needed directories are created
	/// </summary>
	public static void EnsureDirectories()
	{
		Console.WriteLine(TdDir);
		string[] dirs = { TdDir, PacksDir, StickersDir, DecodedDir, ThumbsDir, TempDir };
		foreach (var path in dirs)
		{
			Utils.EnsurePath(path);
		}
	}
}
