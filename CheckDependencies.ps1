$errorActionPreference = 'SilentlyContinue'
$rt = Invoke-Expression 'dotnet --list-runtimes'
$appRt = Get-AppxPackage *appruntime*
$winapprt = Get-AppxPackage *Microsoft.WindowsAppRuntime.1.0*
$winappmain = Get-AppxPackage *MicrosoftCorporationII.WindowsAppRuntime.Main.1.0*
$winappsingleton = Get-AppxPackage *Microsoft.WindowsAppRuntime.Singleton*
$winappddlm = Get-AppxPackage *Microsoft.WinAppRuntime.DDLM.*


$runtimesInstalled = $rt -like '*Microsoft.WindowsDesktop.App 6.0*'
$allPackagesInstalled = $winapprt -and $winappmain -and $winappsingleton -and $winappddlm
$anyPackageInstalled = $appRt -like '*Microsoft.WindowsAppRuntime.*'

if (!$runtimesInstalled) {
    ''
    Write-Host '================================================================================' -ForegroundColor Red
    Write-Host 'The .NET 6 Desktop Runtime is not installed. Please visit this link to download.' -ForegroundColor Red
    'https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime'
    ''
    'Please remember to download the Desktop Runtime.'
    Write-Host '================================================================================' -ForegroundColor Red
    ''
}

if ($anyPackageInstalled -and !$allPackagesInstalled) {
    ''
    Write-Host '================================================================================' -ForegroundColor Yellow
    Write-Host 'A Windows App Runtime package was found, but we cannot verify if it is the correct version.' -ForegroundColor Yellow
    Write-Host 'The app may still work properly without the correct version of the Windows App Runtime, but if not, please redownload the installer here'
    'https://aka.ms/windowsappsdk/1.0-stable/msix-installer'
    ''
    'Please install the correct version for your OS. This app is built for x64.'
    Write-Host '================================================================================' -ForegroundColor Yellow
    ''
}

if (!$anyPackageInstalled) {
    ''
    Write-Host '================================================================================' -ForegroundColor Red
    Write-Host 'The Windows App Runtime is not installed. Please visit this link to download.' -ForegroundColor Red
    'https://aka.ms/windowsappsdk/1.0-stable/msix-installer'
    Write-Host '================================================================================' -ForegroundColor Red
    ''
}

if ($allPackagesInstalled -and $runtimesInstalled) {
   Write-Host  'All dependencies should be installed! Happy stickering! 🐼' -ForegroundColor Green
    ''
}

pause