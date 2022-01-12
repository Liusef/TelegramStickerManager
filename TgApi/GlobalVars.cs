using TdLib;

namespace TgApi;

public class GlobalVars
{
    public static TdClient Client;
    
    public static char Sep = Path.DirectorySeparatorChar;
    
    public static readonly int ApiId = ApiKeys.ApiId;
    public static readonly string ApiHash = ApiKeys.ApiHash;

	public const string ApplicationVersion = "0.0.1";
    public const string DeviceModel = "PC";
    public const string SystemLanguageCode = "en";
    
    public const string StickerBot = "Stickers";

    public static string TdDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Sep}TgStickers{Sep}";
    public static string PacksDir = $"{TdDir}documents{Sep}";

    public static string PacksFileName = "packs.json";
    
    public static void EnsureDirectories()
    {
        Console.WriteLine(TdDir);
        string[] dirs = {TdDir, PacksDir};
        foreach (string path in dirs)
        {
            Utils.EnsurePath(path);
        }
    }
}
