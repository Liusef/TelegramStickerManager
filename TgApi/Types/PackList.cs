using TdLib;
using TgApi.Telegram;

namespace TgApi.Types;

public static class PackList
{
    public static async Task<string[]> GetOwnedPacks(TdClient client)
    {
        if (IsInCache()) return ReadCache();
        return await client.GetOwnedPacksAsync();
    }

    public static void Cache(string[] input) =>
        Utils.Serialize<string[]>(input, $"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");

    public static bool IsInCache() => File.Exists($"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");

    public static string[] ReadCache() =>
        Utils.Deserialize<string[]>($"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");
}
