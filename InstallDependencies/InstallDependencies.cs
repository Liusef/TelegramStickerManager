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
		private const string Net6DRT = "Microsoft.WindowsDesktop.App 6.0";
		private const string WinART64 = "Microsoft.WindowsAppRuntime.1.0";
		private const string WinARTMain = "MicrosoftCorporationII.WindowsAppRuntime.Main.1.0";
		private const string WinARTSingleton = "Microsoft.WindowsAppRuntime.Singleton";
		private const string WinARTDDLM = "Microsoft.WinAppRuntime.DDLM.1.440.209.0-x6";
		private const string AppRT = "*appruntime*";
		private const string WinARTVer = "1.440.209.0";

		private const string NET6Download = "https://download.visualstudio.microsoft.com/download/pr/7fbe3ce3-4082-4995-93de-674038ac919b/56d3fa94d78dc3f39fc70d73ef174c93/windowsdesktop-runtime-6.0.2-win-x64.exe";
		private const string WinAppRTDownload = "https://aka.ms/windowsappsdk/1.0/1.0.1/windowsappruntimeinstall-1.0.1-x64.exe";

		public static async Task Main(string[] args)
		{
			Console.Write("\nChecking if you have required dependencies installed");

			bool NET6 = false;
			bool NET6Canceled = false;
			bool WARTFull = false;
			bool WARTCanceled = false;
			bool WARTAny = false;
			bool WARTAnyCanceled = false;

			var t = Task.Run(() =>
			{
				try
				{
					NET6 = Net6DRTInstalled();
				}
				catch
				{
					NET6 = false;
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

			if (!NET6)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("We couldn't find the .NET 6 Desktop Runtime on your device\n");
				Console.ResetColor();

				if (Utils.PromptYesNo("Would you like this program to automatically install the .NET 6 Desktop Runtime (54.5 MB)? (y/n): "))
				{
					var s = await InstallNET6DRT();
					NET6 = s;
					NET6Canceled = !s;
				}
				else
				{
					NET6Canceled = true;
				}
			}

			if (!WARTFull && WARTAny)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("We found a version a version of the Windows App Runtime, but it may be incompatible.\n");
				Console.ResetColor();

				if (Utils.PromptYesNo("Would you like this program to automatically install the Windows App Runtime 1.0 (215 MB)? (y/n): "))
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
				Console.WriteLine("We couldn't find the Windows App Runtime on your device\n");
				Console.ResetColor();

				if (Utils.PromptYesNo("Would you like this program to automatically install the Windows App Runtime 1.0 (215 MB)? (y/n): "))
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


			if (NET6Canceled)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n{Sep}\nThe .NET 6 Desktop Runtime is required to run the application. Download it here:");
				Console.ResetColor();
				Console.WriteLine("https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime\n\n" +
								  "Please remember to download the .NET 6 Desktop Runtime for running Desktop Apps.");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(Sep);
				Console.ResetColor();
			}
			if (WARTCanceled)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n{Sep}\nThe Windows App Runtime is required to run the application. Download it here:");
				Console.ResetColor();
				Console.WriteLine("https://aka.ms/windowsappsdk/1.0-stable/msix-installer");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(Sep);
				Console.ResetColor();
			}
			else if (WARTAnyCanceled)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"\n{ Sep}\nThe app may not run with your Windows App Runtime installation. Download the recommended version here:");
				Console.ResetColor();
				Console.WriteLine("https://aka.ms/windowsappsdk/1.0-stable/msix-installer");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(Sep + "\n");
				Console.ResetColor();
			}
			if (NET6 && WARTFull)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\nAll dependencies should be installed! Happy stickering!\n");
				Console.ResetColor();
			}

			Console.WriteLine();
			Utils.Pause();
		}

		public static bool Net6DRTInstalled(string exe = "dotnet")
		{
			var s = Utils.GetConsoleOut(exe, "--list-runtimes");
			return s.Contains(Net6DRT);
		}

		public static bool WinARTFullInstall()
		{
			foreach (string output in new[] { WinART64, WinARTMain, WinARTSingleton, WinARTDDLM })
			{
				var s = Utils.GetConsoleOut("powershell", $"-Command Get-AppxPackage {output}");
				if (!s.Contains(WinARTVer)) return false;
			}
			return true;
		}

		public static bool WinARTAnyInstall()
		{
			return Utils.GetConsoleOut("powershell", $"-Command Get-AppxPackage {AppRT}").Length != 0;
		}

		public static async Task<bool> InstallNET6DRT()
		{
			var localPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}net6_0_102_desktop_runtime.exe";
			var t = Task.Run(async () =>
			{
				using (var wc = new WebClient())
				{
					await wc.DownloadFileTaskAsync(NET6Download, localPath);
				}
			});
			Console.Write("Downloading the .NET 6 Desktop Runtime (54.5 MB). This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			Console.WriteLine();
			if (!File.Exists(localPath))
			{
				var c1 = Utils.PromptYesNo("The .NET 6 Desktop Runtime was not downloaded successfully. Would you like to continue anyways? (y/n):");
				if (c1) return false;
				Environment.Exit(1);
			}
			t = Task.Run(() => Utils.RunProcess(localPath, "/passive"));
			Console.Write("Installing the .NET 6 Desktop Runtime. This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			bool done = false;
			try
			{
				done = Net6DRTInstalled("C:\\Program Files\\dotnet\\dotnet.exe");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			if (done)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n.NET 6 Desktop Runtime installed successfully! Continuing...");
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
			var localpath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}winapprt_1.0.1_x64.exe";
			var t = Task.Run(async () =>
			{
				using (var wc = new WebClient())
				{
					await wc.DownloadFileTaskAsync(WinAppRTDownload, localpath);
				}
			});
			Console.Write("Downloading the Windows App Runtime (54.2 MB). This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			Console.WriteLine();
			if (!File.Exists(localpath))
			{
				var c1 = Utils.PromptYesNo("The Windows App Runtime was not downloaded successfully. Would you like to continue anyways? (y/n):");
				if (c1) return false;
				Environment.Exit(1);
			}
			t = Task.Run(() => Utils.RunAsAdmin(localpath, ""));
			Console.Write("Installing the Windows App Runtime. This may take a while");
			while (!t.IsCompleted)
			{
				Console.Write(".");
				await Task.Delay(1000);
			}
			bool done = WinARTFullInstall();
			if (done)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\nWindows App Runtime installed successfully! Continuing...");
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
