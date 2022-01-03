# TelegramStickerManager
An app to deal with @Stickers so you don't have to

## Setting up the project
To download the code and run on your local machine do the following:

1. Create a file called `ApiKeys.cs` in the root of the TgApi project to include your API Keys. The program won't run without them! Contents of the file should be as follows

    ```c#
    namespace TgApi;
    
    public class ApiKeys
    {
    public const int ApiId = 0; // API ID goes here
    public const string ApiHash = ""; // API Hash string goes here
    }
    
    ```
2. Download required packages using `dotnet restore`  