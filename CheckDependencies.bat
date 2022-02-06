@echo off
powershell -Command "&{$rt = Invoke-Expression 'dotnet --list-runtimes'; $wasdk = get-appxpackage *appruntime*; $runtimesInstalled = $rt -like '*Microsoft.WindowsDesktop.App 6*'; $packagedInstalled = $wasdk -like '*Microsoft.WindowsAppRuntime.*'; if (!$runtimesInstalled) {; ''; '================================================================================'; 'The .NET 6 Desktop Runtime is not installed. Please visit this link to download.'; 'https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime'; ''; 'Please remember to download the Desktop Runtime.'; '================================================================================'; ''; }; if (!$packagedInstalled) {; ''; '================================================================================'; 'The Windows App Runtime is not installed. Please visit this link to download.'; 'https://aka.ms/windowsappsdk/1.0-stable/msix-installer'; '================================================================================'; ''; }; if ($packagedInstalled -and $runtimesInstalled) {; ''; 'All dependencies should be installed! Happy stickering!'; ''; }; }"
pause