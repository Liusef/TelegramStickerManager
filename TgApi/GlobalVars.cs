using TdLib;

namespace TgApi;

public class GlobalVars
{
    public static readonly TdClient Client = new TdClient();
    
    public static readonly char Sep = Path.DirectorySeparatorChar;
    
    public static readonly int ApiId = ApiKeys.ApiId;
    public static readonly string ApiHash = ApiKeys.ApiHash;
    
    public static readonly TdApi.TdlibParameters TdParams = new TdApi.TdlibParameters
    {
        ApiId = ApiId,
        ApiHash = ApiHash,
        SystemLanguageCode = "en",
        DeviceModel = "Computer",
        ApplicationVersion = "0.0.1",
        DatabaseDirectory = TdDir,
        UseFileDatabase = true,
        UseMessageDatabase = false,
        UseSecretChats = false
    };
    
    public const string StickerBot = "Stickers";
    
    public static readonly string SaveDir = $"Tgsticker{Sep}";
    public static readonly string TdDir = $"{SaveDir}Td{Sep}";
    public static readonly string CacheDir = $"{SaveDir}Cache{Sep}";

    public static readonly string PacksFileName = "packs.json";
    
    public static void EnsureDirectories()
    {
        string[] dirs = {SaveDir, TdDir, CacheDir};
        foreach (string path in dirs)
        {
            Utils.EnsurePath(path);
        }
    }
}
