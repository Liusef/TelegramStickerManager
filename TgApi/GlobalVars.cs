using TdLib;

namespace TgApi;

public class GlobalVars
{
    
    private static char Sep = Path.DirectorySeparatorChar;

    public static readonly int ApiId = ApiKeys.ApiId;
    public static readonly string ApiHash = ApiKeys.ApiHash;

    public const string ApplicationVersion = "0.0.1";
    public const string DeviceModel = "PC";
    public const string SystemLanguageCode = "en";

    public const string StickerBot = "Stickers";

    public static string TdDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Sep}TgStickers{Sep}";
    public static string PacksDir = $"{TdDir}documents{Sep}";
    public static string StickersDir = $"{TdDir}stickers{Sep}";
    public static string DecodedDir = $"{StickersDir}decoded{Sep}";
    public static string ThumbsDir = $"{TdDir}thumbnails{Sep}";
    public static string TempDir = $"{TdDir}temp{Sep}";

    public static string PacksFileName = "packs.json";
    
    /// <summary>
    /// Ensures that all needed directories are created
    /// </summary>
    public static void EnsureDirectories()
    {
        Console.WriteLine(TdDir);
        string[] dirs = {TdDir, PacksDir, StickersDir, DecodedDir, ThumbsDir, TempDir};
        foreach (string path in dirs)
        {
            Utils.EnsurePath(path);
        }
    }
}
