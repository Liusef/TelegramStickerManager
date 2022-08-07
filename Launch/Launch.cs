using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launch
{
    internal static class Launch
    {
        internal static readonly string appPath = $"{AppDomain.CurrentDomain.BaseDirectory}TgStickers\\ReunionApp.exe";
        internal static readonly string localPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}net_desktop_runtime.exe";
        internal const string NETRuntimeTitle = ".NET 6 Desktop Runtime";
        internal const string NETRuntimeName = "Microsoft.WindowsDesktop.App 6.0";
        internal const string NETRuntimeSize = "55.0MB";
        internal const string NETRuntimeURI = "https://download.visualstudio.microsoft.com/download/pr/9d6b6b34-44b5-4cf4-b924-79a00deb9795/2f17c30bdf42b6a8950a8552438cf8c1/windowsdesktop-runtime-6.0.6-win-x64.exe";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!RuntimePresent()) NotFound();
            else Finish();
        }

        internal static bool RuntimePresent()
        {
            try
            {
                return Utils.GetConsoleOut("dotnet", "--list-runtimes").Contains(NETRuntimeName);
            } 
            catch
            {
                return false;
            }
        }

        internal static void NotFound()
        {
            SystemSounds.Hand.Play();
            DialogResult dialogResult = MessageBox.Show($"The {NETRuntimeTitle} was not found. \n\nWould you like to download it now? ({NETRuntimeSize})", 
                "Runtime Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
            {
                InstallNET();
            }
            else if (dialogResult == DialogResult.No)
            {
                Environment.Exit(0);
            }
        }

        internal static void InstallNET()
        {
            Application.Run(new NETDownload());
        }

        internal static void Finish() 
        {
            Utils.RunProcess(appPath, "", false);
            Application.Exit();
        }
    }

    internal static class Utils
    {
        internal static string GetConsoleOut(string exe, string args = "")
        {
            Process pr = new Process();
            pr.StartInfo.FileName = exe;
            pr.StartInfo.Arguments = args;
            pr.StartInfo.UseShellExecute = false;
            pr.StartInfo.CreateNoWindow = true;
            pr.StartInfo.RedirectStandardOutput = true;
            pr.Start();
            string output = pr.StandardOutput.ReadToEnd();
            pr.WaitForExit();
            return output;
        }

        internal static void RunProcess(string exe, string args, bool wait)
        {
            Process pr = new Process();
            pr.StartInfo.FileName = exe;
            pr.StartInfo.Arguments = args;
            pr.StartInfo.UseShellExecute = false;
            pr.StartInfo.CreateNoWindow = true;
            pr.Start();
            if (wait) pr.WaitForExit();
        }
    }

}
