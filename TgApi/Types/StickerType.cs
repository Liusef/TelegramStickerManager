namespace TgApi.Types;

/// <summary>
/// The type of a sticker pack.
/// </summary>
public enum StickerType : short
{
	Standard = 0,
	Animated = 1,
	Video = 2,

	Mask = -1
}