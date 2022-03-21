﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReunionApp.Pages.CommandPages;
public static class StickerLogic
{
    public static StickerError[] GetStickerErrors(NewSticker[] stickers)
    {
        var errs = new List<StickerError>();    
        for(int i = 0; i < stickers.Length; i++)
        {
            if (!File.Exists(stickers[i].EnsuredPath))
                errs.Add(new StickerError { Index = i, Message = $"{stickers[i].EnsuredPath}: {LogicConsts.FileNotFound}" });
            var emojierr = GetEmojiError(stickers[i].Emojis, i);
            if (emojierr is not null) errs.Add(emojierr);
        }
        return errs.ToArray();
    }

    public static StickerError GetEmojiError(string emojis, int intendedIndex)
    {
        if (string.IsNullOrWhiteSpace(emojis))
            return new StickerError { Index = intendedIndex, Message = LogicConsts.EmptyEmoji };
        else if (!Emoji.IsEmoji(emojis))
            return new StickerError { Index = intendedIndex, Message = LogicConsts.InvalidEmoji };
        return null;
    }

    public static StickerError[] GetEmojiErrorsList(string[] emojis)
    {
        List<StickerError> errorList = new List<StickerError>();
        for (int i = 0; i < emojis.Length; i++)
        {
            var err = GetEmojiError(emojis[i], i);
            if (err is not null) errorList.Add(err);
        }
        return errorList.ToArray();
    }


    public static async Task<string> ResizeToStickerAsync(string path, bool scale)
    {
        try
        {
            if (scale) return await TgApi.ImgUtils.ResizeFitAsync(path, LogicConsts.StickerSize, LogicConsts.StickerSize, true, LogicConsts.formats);
            else return await TgApi.ImgUtils.ResizeFitPadWidthPriorityAsync(path, LogicConsts.StickerSize, LogicConsts.StickerSize, true, LogicConsts.formats);
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
        }
        return " ";
    }


    public static async Task ResizeToStickerAsync(NewSticker sticker, bool scale)
    {
        sticker.TempPath = await ResizeToStickerAsync(sticker.ImgPath, scale);
    }

    public static async Task ResizeAllToStickerParallelAsync(NewSticker[] stickers, bool scale)
    {
        await Parallel.ForEachAsync(stickers, new ParallelOptions { MaxDegreeOfParallelism = App.Threads },
            async (sticker, ct) => await ResizeToStickerAsync(sticker, scale));
        ImgUtils.CollectImageSharpLater(5000);
    }

    public static async Task ResizeToStickerAsync(ReplaceStickerUpdate sticker, bool scale)
    {
        if (File.Exists(sticker.NewPath)) sticker.ThreadsafeNewPath = await ResizeToStickerAsync(sticker.NewPath, scale);
    }

    public static async Task ResizeAllToStickerParallelAsync(ReplaceStickerUpdate[] stickers, bool scale)
    {
        await Parallel.ForEachAsync(stickers, new ParallelOptions { MaxDegreeOfParallelism = App.Threads },
            async (sticker, ct) => await ResizeToStickerAsync(sticker, scale));
        ImgUtils.CollectImageSharpLater(5000);
    }

    public static async Task<string> ResizeToThumbAsync(string path)
    {
        try
        {
            return await TgApi.ImgUtils.ResizePadAsync(path, LogicConsts.ThumbSize, LogicConsts.ThumbSize, true, LogicConsts.formats);
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
        }
        return " ";
    }
}

public class StickerError
{
    public int Index { get; set; }
    public string Message { get; set; }
    public override string ToString() => $"Error at #{Index+1}: {Message}";
    
}
