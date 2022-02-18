﻿using TgApi.Types;

namespace TestCLI;

public static class StickerSorter
{
    public static bool IsSorted(StickerPack pack, Sticker[] order)
    {
        for (int i = 0; i < pack.Count - 1; i++)
        {
            if (pack.Stickers[i].RemoteFileId != order[i].RemoteFileId) return false;
        }
        return true;
    }
    
    public static (Sticker, Sticker)[] GetSwaps(StickerPack pack, Sticker[] order)
    {
        List<(Sticker, Sticker)> r = new List<(Sticker, Sticker)>();
        List<(int, Sticker)> s = new List<(int, Sticker)>();
        var orderList = new List<Sticker>(order);

        for (int i = 0; i < pack.Count; i++)
        {
            s.Add((orderList.IndexOf(pack.Stickers[i]), pack.Stickers[i]));
        }

        for (int i = 1; i < s.Count; i++)
        {
            var item = s[i];
            int val = item.Item1;
            int j = i - 1;

            while (j >= 0 && s[j].Item1 > val)
            {
                s[j + 1] = s[j];
                j--;
            }
            s[j + 1] = item;
            if (j == 0) // To move something to the first index, move sticker to the right of the first sticker
                        // then move first sticker to the right of the target sticker
            {
                r.Add((item.Item2, s[1].Item2)); // The original 1st item in the pack is now at s[1]
                r.Add((s[1].Item2, item.Item2));
            }
            else r.Add((item.Item2, s[j].Item2));
        }

        return r.ToArray();
    }

}