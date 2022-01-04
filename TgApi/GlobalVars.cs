using TdLib;

namespace TgApi;

public class GlobalVars
{
    // public static readonly TdApi.Client Client = new TdClient();
    
    public static readonly char Sep = Path.DirectorySeparatorChar;
    
    public static readonly int ApiId = ApiKeys.ApiId;
    public static readonly string ApiHash = ApiKeys.ApiHash;
    
    public const string StickerBot = "Stickers";
    
    public static readonly string SaveDir = "Tgsticker";
    public static readonly string UserDir = $"{SaveDir}{Sep}UserData";
    public static readonly string CacheDir = $"{SaveDir}{Sep}Cache";

    public static readonly string PacksFileName = "packs.json";
}
