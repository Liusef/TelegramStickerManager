using System.Collections.Generic;
using TgApi.Types;

namespace ReunionApp.Runners.RunnerDependencies;

/// <summary>
/// Sorts stickers according to a specified order 
/// </summary>
public static class OrderStickerRunnerSorter
{
    /// <summary>
    /// Whether or not the pack is already sorted to how the user wants
    /// </summary>
    /// <param name="pack">The StickerPack in question</param>
    /// <param name="order">The order desired by the user</param>
    /// <returns>Whether or not the pack is already sorted</returns>
    public static bool IsSorted(StickerPack pack, Sticker[] order)
    {
        for (int i = 0; i < pack.Count - 1; i++)
        {
            if (pack.Stickers[i].RemoteFileId != order[i].RemoteFileId) return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the swaps needed to order the sticker pack to how the user wants it. Uses insertion sort.
    /// </summary>
    /// <param name="pack">The sticker pack that needs to be sorted</param>
    /// <param name="order">The order that the user wants</param>
    /// <returns>A list of (Sticker, Sticker) tuples that indicate swaps on @Stickers</returns>
    public static (Sticker, Sticker)[] GetSwaps(StickerPack pack, IEnumerable<Sticker> order)
    {
        List<(Sticker, Sticker)> r = new List<(Sticker, Sticker)>();
        List<(int, Sticker)> s = new List<(int, Sticker)>();
        var orderList = new List<Sticker>(order);

        for (int i = 0; i < pack.Count; i++) s.Add((orderList.IndexOf(pack.Stickers[i]), pack.Stickers[i]));
        
        for (int i = 1; i < s.Count; i++)
        {
            var item = s[i];
            int val = item.Item1;
            int j = i - 1;

            while (j >= 0 && s[j].Item1 > val) // Insertion sort
            {
                s[j + 1] = s[j];
                j--;
            }
            s[j + 1] = item;

            if (j != i - 1)
            {       
                if (j < 0)                               
                {                                       // To move something to the first index, move sticker to the right of the first
                    r.Add((item.Item2, s[1].Item2));    // then move first sticker to the right of the target sticker
                    r.Add((s[1].Item2, item.Item2));    // The original 1st item in the pack is now at s[1]
                }
                else r.Add((item.Item2, s[j].Item2));
            }
        }
        return r.ToArray();
    }
}
