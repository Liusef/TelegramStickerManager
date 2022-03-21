using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoSmart.Unicode;
using TgApi;


namespace ReunionApp.Pages.CommandPages;

public static class LogicConsts
{
    public const int StickerSize = 512;
    public const int ThumbSize = 100;

    public const string EmptyEmoji = "The Emojis string cannot be empty!";
    public const string InvalidEmoji = "The Emojis string contains non-emoji characters";
    public const string FileNotFound = "The file associated with this sticker could not be found";

    public static readonly string[] formats = new[] { "png", "webp" };
}

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


    public static async Task<string> ResizeToStickerAsync(string path)
    {
        try
        {
            return await TgApi.ImgUtils.ResizeFitAsync(path, LogicConsts.StickerSize, LogicConsts.StickerSize, true, LogicConsts.formats);
        }
        catch (Exception ex)
        {
            await App.GetInstance().ShowExceptionDialog(ex);
        }
        return " ";
    }

    public static async Task ResizeToStickerAsync(NewSticker sticker)
    {
        sticker.TempPath = await ResizeToStickerAsync(sticker.ImgPath);
    }

    public static async Task ResizeAllToStickerParallelAsync(NewSticker[] stickers)
    {
        await Parallel.ForEachAsync(stickers, new ParallelOptions { MaxDegreeOfParallelism = App.Threads },
            async (sticker, ct) => await ResizeToStickerAsync(sticker));
        ImgUtils.CollectImageSharpLater(5000);
    }

    public static async Task<string> ResizeToThumbAsync(string path)
    {
        try
        {
            // TODO update this to be the one that doesn't distort everything
            return await TgApi.ImgUtils.ResizeAsync(path, LogicConsts.ThumbSize, LogicConsts.ThumbSize, true, LogicConsts.formats);
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
