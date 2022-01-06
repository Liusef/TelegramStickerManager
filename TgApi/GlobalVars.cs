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
        UseFileDatabase = true
    };
    
    public const string StickerBot = "Stickers";

    public static readonly string TdDir = $"Td{Sep}";
    public static readonly string PacksDir = $"{TdDir}documents{Sep}";

    public static readonly string PacksFileName = "packs.json";
    
    public static void EnsureDirectories()
    {
        string[] dirs = {TdDir, PacksDir};
        foreach (string path in dirs)
        {
            Utils.EnsurePath(path);
        }
    }
}
