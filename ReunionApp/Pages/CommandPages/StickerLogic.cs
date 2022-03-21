using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoSmart.Unicode;

namespace ReunionApp.Pages.CommandPages;
public static class StickerLogic
{
    public static StickerError[] GetStickerErrors(NewSticker[] stickers)
    {
        var errs = new List<StickerError>();    
        for(int i = 0; i < stickers.Length; i++)
        {
            if (!File.Exists(stickers[i].EnsuredPath))
                errs.Add(new StickerError { Index = i, Message = $"{stickers[i].EnsuredPath}: {StickerError.FileNotFound}" });
            var emojierr = GetEmojiError(stickers[i].Emojis, i);
            if (emojierr is not null) errs.Add(emojierr);
        }
        return errs.ToArray();
    }

    public static StickerError GetEmojiError(string emojis, int intendedIndex)
    {
        if (string.IsNullOrWhiteSpace(emojis))
            return new StickerError { Index = intendedIndex, Message = StickerError.EmptyEmoji };
        else if (!Emoji.IsEmoji(emojis))
            return new StickerError { Index = intendedIndex, Message = StickerError.InvalidEmoji };
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


}

public class StickerError
{
    public const string EmptyEmoji = "The Emojis string cannot be empty!";
    public const string InvalidEmoji = "The Emojis string contains non-emoji characters";
    public const string FileNotFound = "The file associated with this sticker could not be found";
    public int Index { get; set; }
    public string Message { get; set; }
    public override string ToString() => $"Error at #{Index+1}: {Message}";
    
}
