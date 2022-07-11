using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launch
{
    public partial class NETDownload : Form
    {

        bool finished = false;

        public NETDownload()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void NETDownload_Load(object sender, EventArgs e)
        {
            var web = new WebClient();
            web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(web_DownloadProgressChanged);
            web.DownloadFileAsync(new Uri(Launch.NETRuntimeURI), Launch.localPath);
        }

        public void web_DownloadProgressChanged(Object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            if (e.ProgressPercentage == 100 && !finished)
            {
                finished = true;

                // Decrementing to 99 because decrementing doesn't have an animation
                // This allows the bar to show as at least almost full when downloading on a fast connection
                progressBar1.Value = 99;
                progressBar1.Value = 100;

                if (!File.Exists(Launch.localPath))
                {
                    MessageBox.Show("The file was not successfully downloaded. Please try again later.", "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                Utils.RunProcess(Launch.localPath, "/passive", true);

                if (!Launch.RuntimePresent())
                {
                    MessageBox.Show($"The {Launch.NETRuntimeTitle} was not installed successfully. Please try again later.", "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                Launch.Finish();
            }
        }
    }
}
