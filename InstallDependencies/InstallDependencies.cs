using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace InstallDependencies
{
	internal static class InstallDependencies
	{
		private const string Sep = "=========================================================";

		private const string NetDRTName = "The .NET 6 Desktop Runtime";
		private const string NetDRTPackage = "Microsoft.WindowsDesktop.App 6.0";

		private const string NETInfoUri = "https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime";
		private const string NETDownloadName = "The .NET Desktop Runtime 6.0.2";
		private const string NETDownloadUri = "https://download.visualstudio.microsoft.com/download/pr/7fbe3ce3-4082-4995-93de-674038ac919b/56d3fa94d78dc3f39fc70d73ef174c93/windowsdesktop-runtime-6.0.2-win-x64.exe";
		private const string NETDownloadSize = "54.5 MB";



		private const string WinARTName = "The Windows App Runtime";
		private const string WinART64 = "Microsoft.WindowsAppRuntime.1.0";
		private const string WinARTMain = "MicrosoftCorporationII.WindowsAppRuntime.Main.1.0";
		private const string WinARTSingleton = "Microsoft.WindowsAppRuntime.Singleton";
		private const string WinARTDDLM = "Microsoft.WinAppRuntime.DDLM.1.440.209.0-x6";
		private const string AppRT = "*appruntime*";
		private const string WinARTPackageVer = "1.440.209.0";

		private const string WinAppRTInfoUri = "https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads";
		private const string WinAppRTDownloadName = "The Windows App Runtime 1.0.1";
		private const string WinAppRTDownloadUri = "https://aka.ms/windowsappsdk/1.0/1.0.1/windowsappruntimeinstall-1.0.1-x64.exe";
		private const string WinAppRTDownloadSize = "53.2 MB";

		public static async Task Main(string[] args)
		{
			Console.Write("\nChecking if you have required dependencies installed");

			bool NET = false;
			bool NETCanceled = false;
			bool WARTFull = false;
			bool WARTCanceled = false;
			bool WARTAny = false;
			bool WARTAnyCanceled = false;

			var t = Task.Run(() =>
			{
				try
				{
					NET = NetDRTInstalled();
				}
				catch
				{
					NET = false;
				}
				WARTFull = WinARTFullInstall();
				WARTAny = WinARTAnyInstall();
			});

			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			await t;

			Console.WriteLine("\n");

			if (!NET)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"We couldn't find {NetDRTName} on your device\n");
				Console.ResetColor();

				if (Utils.PromptYesNo($"Would you like this program to automatically install {NETDownloadName} ({NETDownloadSize})? (y/n): "))
				{
					var s = await InstallNETDRT();
					NET = s;
					NETCanceled = !s;
				}
				else
				{
					NETCanceled = true;
				}
			}

			if (!WARTFull && WARTAny)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"We found a version a version of {WinARTName}, but it may be incompatible.\n");
				Console.ResetColor();

				if (Utils.PromptYesNo($"Would you like this program to automatically install {WinAppRTDownloadName} ({WinAppRTDownloadSize})? (y/n): "))
				{
					var s = await InstallWART();
					WARTFull = s;
					WARTCanceled = !s;
				}
				else
				{
					WARTAnyCanceled = true;
				}
			}

			else if (!WARTFull && !WARTAny)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"We couldn't find {WinARTName} on your device\n");
				Console.ResetColor();

				if (Utils.PromptYesNo($"Would you like this program to automatically install {WinAppRTDownloadName} ({WinAppRTDownloadSize})? (y/n): "))
				{
					var s = await InstallWART();
					WARTFull = s;
					WARTCanceled = !s;
				}
				else
				{
					WARTCanceled = true;
				}
			}


			if (NETCanceled)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n{Sep}\n{NetDRTName} is required to run the application. Download it here:");
				Console.ResetColor();
				Console.WriteLine($"{NETInfoUri}\n\n" +
								  "Please remember to download the Desktop Runtime for running Desktop Apps.");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(Sep);
				Console.ResetColor();
			}
			if (WARTCanceled)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n{Sep}\n{WinARTName} is required to run the application. Download it here:");
				Console.ResetColor();
				Console.WriteLine(WinAppRTInfoUri);
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(Sep);
				Console.ResetColor();
			}
			else if (WARTAnyCanceled)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"\n{ Sep}\nThe app may not run with your installation of {WinARTName}. Download the recommended version here:");
				Console.ResetColor();
				Console.WriteLine(WinAppRTInfoUri);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(Sep + "\n");
				Console.ResetColor();
			}
			if (NET && WARTFull)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\nAll dependencies should be installed! Happy stickering!\n");
				Console.ResetColor();
			}

			Console.WriteLine();
			Utils.Pause();
		}

		public static bool NetDRTInstalled(string exe = "dotnet")
		{
			var s = Utils.GetConsoleOut(exe, "--list-runtimes");
			return s.Contains(NetDRTPackage);
		}

		public static bool WinARTFullInstall()
		{
			foreach (string output in new[] { WinART64, WinARTMain, WinARTSingleton, WinARTDDLM })
			{
				var s = Utils.GetConsoleOut("powershell", $"-Command Get-AppxPackage {output}");
				if (!s.Contains(WinARTPackageVer)) return false;
			}
			return true;
		}

		public static bool WinARTAnyInstall()
		{
			return Utils.GetConsoleOut("powershell", $"-Command Get-AppxPackage {AppRT}").Length != 0;
		}

		public static async Task<bool> InstallNETDRT()
		{
			var localPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}net_desktop_runtime.exe";
			var t = Task.Run(async () =>
			{
				using (var wc = new WebClient())
				{
					await wc.DownloadFileTaskAsync(NETDownloadUri, localPath);
				}
			});
			Console.Write($"Downloading {NETDownloadName} ({NETDownloadSize}). This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			Console.WriteLine();
			if (!File.Exists(localPath))
			{
				var c1 = Utils.PromptYesNo($"{NETDownloadName} was not downloaded successfully. Would you like to continue anyways? (y/n):");
				if (c1) return false;
				Environment.Exit(1);
			}
			t = Task.Run(() => Utils.RunProcess(localPath, "/passive"));
			Console.Write($"Installing {NETDownloadName}. This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			bool done = false;
			try
			{
				done = NetDRTInstalled("C:\\Program Files\\dotnet\\dotnet.exe");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			if (done)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"\n.{NETDownloadName} installed successfully! Continuing...");
				Console.ResetColor();
				return true;
			}
			else
			{
				Console.WriteLine("\nInstallation was unsuccessful. Re-run this app later to see if Installation was successful. Continuing...");
				return false;
			}
		}

		public static async Task<bool> InstallWART()
		{
			var localpath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}windowsappruntime.exe";
			var t = Task.Run(async () =>
			{
				using (var wc = new WebClient())
				{
					await wc.DownloadFileTaskAsync(WinAppRTDownloadUri, localpath);
				}
			});
			Console.Write($"Downloading {WinAppRTDownloadName} ({WinAppRTDownloadSize}). This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			Console.WriteLine();
			if (!File.Exists(localpath))
			{
				var c1 = Utils.PromptYesNo($"{WinAppRTDownloadName} was not downloaded successfully. Would you like to continue anyways? (y/n):");
				if (c1) return false;
				Environment.Exit(1);
			}
			t = Task.Run(() => Utils.RunAsAdmin(localpath, ""));
			Console.Write($"Installing {WinAppRTDownloadName}. This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			bool done = WinARTFullInstall();
			if (done)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"\n{WinAppRTDownloadName} was installed successfully! Continuing...");
				Console.ResetColor();
				return true;
			}
			else
			{
				Console.WriteLine("\nInstallation was unsuccessful. Re-run this app later to see if Installation was successful. Continuing...");
				return false;
			}
		}

	}

	public static class Utils
	{
		public static string Prompt(string prompt)
		{
			Console.Write(prompt);
			return Console.ReadLine();
		}

		public static bool PromptYesNo(string prompt)
		{
			while (true)
			{
				var s = Prompt(prompt).Trim().ToLower();
				if (s == "y") return true;
				if (s == "n") return false;
				Console.WriteLine("The entered input was invalid.\n");
			}
		}

		public static bool IsAdmin()
		{
			bool isAdmin;
			try
			{
				WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (Exception)
			{
				isAdmin = false;
			}
			return isAdmin;
		}

		public static void RunProcess(string exe, string args)
		{
			Process pr = new Process();
			pr.StartInfo.FileName = exe;
			pr.StartInfo.Arguments = args;
			pr.StartInfo.UseShellExecute = false;
			pr.Start();
			pr.WaitForExit();
		}

		public static void RunAsAdmin(string exe, string args)
		{
			Process pr = new Process();
			pr.StartInfo.FileName = exe;
			pr.StartInfo.Arguments = args;
			pr.StartInfo.UseShellExecute = true;
			pr.StartInfo.Verb = "runas";
			pr.Start();
			pr.WaitForExit();
		}

		public static string GetConsoleOut(string exe, string args)
		{
			Process pr = new Process();
			pr.StartInfo.FileName = exe;
			pr.StartInfo.Arguments = args;
			pr.StartInfo.UseShellExecute = false;
			pr.StartInfo.RedirectStandardOutput = true;
			pr.Start();
			var r = pr.StandardOutput.ReadToEnd();
			pr.WaitForExit();
			return r;
		}

		public static void Pause() => Prompt("Press Enter to continue...");
	}
}
