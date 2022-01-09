# TelegramStickerManager
An app to deal with @Stickers so you don't have to

## Setting up the project
To download the code and run on your local machine do the following:

1. Install Visual Studio 2022 and the .NET 6 SDK
2. Follow the instructions to [install all developer tools](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=vs-2022)
3. Download and Install the [Windows App Runtime.](https://aka.ms/windowsappsdk/1.0-stable/msix-installer) If installation fails, you may have to run the desired .exe from an admin command line.
4. Download required packages using `dotnet restore`  for the TgApi and TestCLI projects.
5. Create a file called `ApiKeys.cs` in the root of the TgApi project to include your API Keys. The program won't run without them! Contents of the file should be as follows

    ```c#
    namespace TgApi;
    
    public class ApiKeys
    {
    public const int ApiId = 0; // API ID goes here
    public const string ApiHash = ""; // API Hash string goes here
    }
    
    ```

To run the final version, you should only need to have the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime) and the [Windows App Runtime](https://aka.ms/windowsappsdk/1.0-stable/msix-installer). 

