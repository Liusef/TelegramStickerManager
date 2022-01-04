using TdLib;

namespace TgApi.Telegram;

public static class Common
{
    public static async Task<long> GetIdFromUsernameAsync(this TdApi.Client client, string username) => 
        (await client.SearchPublicChatAsync(username)).Id;
    
}
