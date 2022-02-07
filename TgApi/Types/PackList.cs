using TdLib;
using TgApi.Telegram;

namespace TgApi.Types;

public static class PackList
{
    /// <summary>
    /// Gets a list of all packs owned by the user
    /// </summary>
    /// <param name="client">An active TdClient</param>
    /// <returns>A string of all the names of owned packs</returns>
    public static async Task<string[]> GetOwnedPacks(TdClient client)
    {
        if (IsInCache()) return ReadCache();
        return await client.GetOwnedPacksAsync();
    }

    /// <summary>
    /// Caches the list of packs to the system
    /// </summary>
    /// <param name="input"></param>
    public static void Cache(string[] input) =>
        Utils.Serialize<string[]>(input, $"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");

    /// <summary>
    /// Whether or not a list of all owned packs is cached
    /// </summary>
    /// <returns>Whether or not a list of all owned packs is cached</returns>
    public static bool IsInCache() => File.Exists($"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");

    /// <summary>
    /// Reads a list of all owned packs from the system
    /// </summary>
    /// <returns>An array of all owned packs</returns>
    public static string[] ReadCache() =>
        Utils.Deserialize<string[]>($"{GlobalVars.PacksDir}{GlobalVars.PacksFileName}");
}
