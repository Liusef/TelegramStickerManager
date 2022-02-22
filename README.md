# TelegramStickerManager
An app to deal with @Stickers so you don't have to.

This program can automate: 
- Making new sticker packs
- Adding stickers
- Deleting stickers
- Re-ordering stickers
- More on the way!

all with a simple drag-and-drop user interface which allows you to save time and avoid having to send hundreds of messages to Sticker bot.

If you find this useful, please star the repository!

## To run as a user:
Check the [releases](https://github.com/Liusef/TelegramStickerManager/releases) page for binaries and run `InstallDependencies.exe` before using!.

To run this application as an end user, you must have at least the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime) and the [Windows App Runtime](https://aka.ms/windowsappsdk/1.0-stable/msix-installer) installed on your machine.

`InstallDependencies.exe` will automatically check and install all dependencies to run.

## Setting up the project
To download the code, compile, and run on your local machine do the following:

1. Install Visual Studio 2022 and the .NET 6 SDK
2. Follow the instructions to [install all developer tools](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=vs-2022)
3. Download and Install the [Windows App Runtime.](https://aka.ms/windowsappsdk/1.0-stable/msix-installer) If installation fails, you may have to run the desired .exe from an admin command line.
4. Download required packages by restoring all projects. This should pull most, if not all, required packages from NuGet or other sources.
5. Create a file called `ApiKeys.cs` in the root of the TgApi project to include your API Keys. The program won't run without them! Contents of the file should be as follows

    ```c#
    namespace TgApi;
    
    public class ApiKeys
    {
        public const int ApiId = 0; // API ID goes here
        public const string ApiHash = ""; // API Hash string goes here
    }
    
    ```

### A small disclaimer

I have no idea what i'm doing, don't expect anything to work yet oops.

