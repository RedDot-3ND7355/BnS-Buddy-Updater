using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication6
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        // Path to app
        public string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        // Virtual browser
        WebClient client = new WebClient();
        public Form1()
        {
            /* Initialize Updater */
            InitializeComponent();
            /* Get Local Version of BnS Buddy */
            GrabCurrentVersion();
            /* Get Online Version of BnS Buddy */
            GrabOnlineVersion();
            /* Compare then Validate Versions */
            Compare_n_Validate();
        }

        public void KillApp()
        {
            Process p = Process.GetCurrentProcess();
            p.Kill();
        }

        private void Compare_n_Validate()
        {
            if (metroLabel3.Text == "Offline")
            {
                metroButton3.Text = "Make sure you are online to use the updater.";
            }
            else if (metroLabel4.Text == "Not found")
            {
                metroButton3.Text = "Click to Download BnS Buddy";
                metroButton3.Enabled = true;
            }
            else
            {
                int online = Convert.ToInt32(metroLabel3.Text.Replace(".", ""));
                int offline = Convert.ToInt32(metroLabel4.Text.Replace(".", ""));

                if (online > offline)
                {
                    metroButton3.Text = "Click to Update to " + metroLabel3.Text;
                    metroButton3.Enabled = true;
                }

                if (online < offline)
                {
                    metroButton3.Text = "What in the world are you doing?";
                }

                if (online == offline)
                {
                    metroButton3.Text = "You are on latest update.";
                }
            }
        }

        private void GrabOnlineVersion()
        {
            using (WebClient browser = new WebClient())
            {
                try
                {
                    browser.Headers.Add("user-agent", "BnSBuddy/" + Application.ProductVersion + " (compatible;)");
                    metroLabel3.Text = browser.DownloadString("https://www.nebulahosts.com/BuddyVersion.txt");
                }
                catch
                {
                    metroLabel3.Text = "Offline";
                }
            }
        }

        private void GrabCurrentVersion()
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(AppPath + "\\BnS Buddy.exe");
                metroLabel4.Text = versionInfo.ProductVersion;
            }
            catch
            {
                metroLabel4.Text = "Not found";
            }
        }

        private AutoResetEvent waitbw = new AutoResetEvent(false);
        private void metroButton3_Click(object sender, EventArgs e)
        {
            metroButton3.Enabled = false;
            try
            {
                client.Headers.Add("user-agent", "BnSBuddy/" + Application.ProductVersion + " (compatible;)");
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                client.DownloadFileAsync(new Uri("https://www.nebulahosts.com/BnS%20Buddy%20%5BBy%20Kogaru%5D.rar"), @AppPath + "\\BnS Buddy [By Kogaru].rar");
                
            }
            catch(Exception a)
            {
                Prompt.Popup("Error: Could not download update!" + Environment.NewLine + a.ToString());
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            metroProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IArchive rar = SharpCompress.Archive.Rar.RarArchive.Open(AppPath + "\\BnS Buddy [By Kogaru].rar", Options.None);
            double totalSize = rar.Entries.Where(a => !a.IsDirectory).Sum(a => a.Size);
            metroProgressBar1.Value = 0;
            long completed = 0;
            Directory.CreateDirectory(AppPath + "\\Update");
            foreach (var entry in rar.Entries.Where(a => !a.IsDirectory))
            {
                entry.WriteToDirectory(AppPath + "\\Update", ExtractOptions.Overwrite);
                completed += entry.Size;
                var percentage = completed / totalSize * 100;
                metroProgressBar1.Value = Convert.ToInt32(percentage);
                metroProgressBar1.Refresh();
            }
            rar.Dispose();
            FinishUpdate();
            KillApp();
        }

        private void FinishUpdate()
        {
            client.CancelAsync();
            if (!client.IsBusy)
            {
                client.Dispose();
            }
            try
            {
                DirectoryInfo folder = new DirectoryInfo(AppPath + "\\Update");
                FileInfo[] files = folder.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Name != "MetroFramework.dll")
                    File.Copy(file.FullName, AppPath + "\\" + file.Name, true);
                    File.Delete(file.FullName);
                }
                folder.Delete();
                File.Delete(AppPath + "\\BnS Buddy [By Kogaru].rar");
                StartBnSBuddy();
            }
            catch(Exception e)
            {
                Prompt.Popup("Error: Couldn't copy update files or clear Update folder." + Environment.NewLine + e.ToString());
            }
        }

        private void StartBnSBuddy()
        {
            Process BnSBuddy = new Process();
            BnSBuddy.StartInfo.FileName = AppPath + "\\BnS Buddy.exe";
            BnSBuddy.Start();
        }

        public static class Prompt
        {
            public static void Popup(string Message)
            {
                ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                MetroFramework.Forms.MetroForm prompt = new MetroFramework.Forms.MetroForm()
                {
                    Width = 280,
                    Height = 135,
                    FormBorderStyle = FormBorderStyle.None,
                    Resizable = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly,
                    Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon"))),
                    ControlBox = false,
                    Theme = MetroFramework.MetroThemeStyle.Dark,
                    DisplayHeader = false,
                    TopMost = true,
                    Text = "",
                    StartPosition = FormStartPosition.CenterScreen
                };
                MetroFramework.Controls.MetroLabel textLabel = new MetroFramework.Controls.MetroLabel() { AutoSize = true, Left = 5, Top = 20, Text = Message, Width = 270, Height = 40, TextAlign = ContentAlignment.MiddleCenter, Theme = MetroFramework.MetroThemeStyle.Dark };
                MetroFramework.Controls.MetroButton confirmation = new MetroFramework.Controls.MetroButton() { Text = "Ok", Left = 5, Width = 100, Top = 130, DialogResult = DialogResult.OK, Theme = MetroFramework.MetroThemeStyle.Dark };
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;
                prompt.ShowDialog();
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            KillApp();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }
}
