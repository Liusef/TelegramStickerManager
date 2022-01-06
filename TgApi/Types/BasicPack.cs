using TdLib;

namespace TgApi.Types;

public class BasicPack
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Name { get; set; }
    public bool IsAnimated { get; set; }
    public StickerPackThumb? Thumb { get; set; }

    public async Task<StickerPack> GetFull(TdClient client, int priority = 1) =>
        await StickerPack.Generate(client, await client.GetStickerSetAsync(Id));

    public void SerializeBasic() =>
        Utils.Serialize<BasicPack>(this, $"{GlobalVars.PacksDir}{Name}.json");

}
