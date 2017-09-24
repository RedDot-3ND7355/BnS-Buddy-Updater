using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Compressor;
using SharpCompress.Compressor.Deflate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
        // Bool to tell app is ready
        bool Ready = false;
        // Cert Stuff
        string BnSBuddySerial = "";
        string BnSServerCert = "";
        public Form1()
        {
            /* Initialize Updater */
            InitializeComponent();
            /* Admin Check */
            CheckIsAdministrator();
            /* Validate Legetimacy of the Updater */
            ValidateBuddy();
            /* Get Local Version of BnS Buddy */
            GrabCurrentVersion();
            /* Get Online Version of BnS Buddy */
            GrabOnlineVersion();
            /* Get current settings for auto updater if it exists */
            GrabSettings();
            /* Compare then Validate Versions */
            Compare_n_Validate();
            /* Set updater as readu */
            Ready = true;
        }

        public static bool IsAdministrator()
        {
            // Direct admin check
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void CheckIsAdministrator()
        {
            // Check admin shortcut
            if (IsAdministrator() == false)
            {
                MessageBox.Show("Please run as Admin", "Error: Not admin", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                KillApp();
            }
        }

        private bool ValidateDomain()
        {
            // Get Domain's Unique Serial Number
            if (BnSBuddySerial.Length > 0)
            {
                X509Certificate2 cert = null;
                try
                {
                    var Client = new TcpClient("updates.xxzer0modzxx.net", 443);
                    var DomainCert = new RemoteCertificateValidationCallback(delegate (object snd, X509Certificate certificate, X509Chain chainLocal, SslPolicyErrors sslPolicyErrors)
                    {
                        return true; //Accept every certificate, even if it's invalid
                    });
                    using (var sslStream = new SslStream(Client.GetStream(), true, DomainCert))
                    {
                        sslStream.AuthenticateAsClient("updates.xxzer0modzxx.net");
                        var serverCertificate = sslStream.RemoteCertificate;
                        cert = new X509Certificate2(serverCertificate);
                    }
                }
                catch { return false; }
                if (!(cert == null))
                {
                    BnSServerCert = cert.GetHashCode().ToString();
                }
                // Verify if Hash Code Matches
                if (BnSBuddySerial == BnSServerCert && BnSBuddySerial == "1307602086")
                {
                    return true;
                }
            }
            return false;
        }

        private void ValidateBuddy()
        {
            // Get BnS Buddy's Unique Serial Number
            X509Certificate certificate = null;
            try
            {
                certificate = X509Certificate2.CreateFromSignedFile(Application.ExecutablePath);
                BnSBuddySerial = certificate.GetHashCode().ToString();
                if (BnSBuddySerial != "1307602086") { Prompt.Popup("BnS Buddy Updater signature does not match! Please Delete and get a fresh copy."); KillApp(); }
            }
            catch { Prompt.Popup("BnS Buddy Updater is not signed! Please Delete and get a fresh copy."); KillApp(); }
        }

        private void GrabSettings()
        {
            if (File.Exists(AppPath + "//Settings.ini"))
            {
                if (File.ReadAllText(AppPath + "//Settings.ini").Contains("autoupdate = true"))
                {
                    metroToggle1.Checked = true;
                }
                else
                {
                    metroToggle1.Checked = false;
                }
            }
            else
            {
                metroLabel5.Visible = false;
                metroToggle1.Visible = false;
            }
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

        private static int BinaryMatch(byte[] input, byte[] pattern)
        {
            int sLen = input.Length - pattern.Length + 1;
            for (int i = 0; i < sLen; ++i)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; ++j)
                {
                    if (input[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }

        private void GrabOnlineVersion()
        {
            if (ValidateDomain() == true)
            {
                try
                {
                    string server = "updates.xxzer0modzxx.net";
                    TcpClient clientVAR = new TcpClient(server, 443);
                    var DomainCert = new RemoteCertificateValidationCallback(delegate (object snd, X509Certificate certificate, X509Chain chainLocal, SslPolicyErrors sslPolicyErrors)
                    {
                        return true; //Accept every certificate, even if it's invalid
                    });
                    using (SslStream sslStream = new SslStream(clientVAR.GetStream(), false, DomainCert))
                    {
                        string result = "";
                        sslStream.AuthenticateAsClient(server);
                        clientVAR.SendTimeout = 500;
                        clientVAR.ReceiveTimeout = 1000;
                        // Send request headers
                        var builder = new StringBuilder();
                        builder.AppendLine("GET /BuddyVersion.txt HTTP/1.1");
                        builder.AppendLine("Host: updates.xxzer0modzxx.net");
                        builder.AppendLine("User-Agent: BnSBuddy/" + Application.ProductVersion + " (compatible;)");
                        builder.AppendLine("Connection: close");
                        builder.AppendLine();
                        var header = Encoding.ASCII.GetBytes(builder.ToString());
                        sslStream.WriteAsync(header, 0, header.Length);
                        // receive data
                        using (var memory = new MemoryStream())
                        {
                            sslStream.CopyTo(memory);
                            memory.Position = 0;
                            var data = memory.ToArray();
                            var index = BinaryMatch(data, Encoding.ASCII.GetBytes("\r\n\r\n")) + 4;
                            var headers = Encoding.ASCII.GetString(data, 0, index);
                            memory.Position = index;

                            if (headers.IndexOf("Content-Encoding: gzip") > 0)
                            {
                                using (GZipStream decompressionStream = new GZipStream(memory, CompressionMode.Decompress))
                                using (var decompressedMemory = new MemoryStream())
                                {
                                    decompressionStream.CopyTo(decompressedMemory);
                                    decompressedMemory.Position = 0;
                                    result = Encoding.UTF8.GetString(decompressedMemory.ToArray());
                                }
                            }
                            else
                            {
                                result = Encoding.UTF8.GetString(data, index, data.Length - index);
                            }
                        }
                        metroLabel3.Text = result;
                    }
                    clientVAR.Close();
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
        
        BackgroundWorker bw = new BackgroundWorker();
        async Task DownloadUpdate(string rootpath)
        {
            try
            {
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "BnSBuddy/" + Application.ProductVersion + " (compatible;)");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = await client.GetAsync("https://updates.xxzer0modzxx.net/BnS%20Buddy%20[By%20Kogaru].rar");
                response.EnsureSuccessStatusCode();
                using (FileStream fileStream = new FileStream(rootpath + "\\BnS Buddy [By Kogaru].rar", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    await response.Content.CopyToAsync(fileStream);
                    while (response.Content.Headers.ContentLength > fileStream.Position)
                    {
                        metroProgressBar1.Maximum = 200;
                        metroProgressBar1.Value = (Convert.ToInt32(fileStream.Position) / Convert.ToInt32(response.Content.Headers.ContentLength)) * 100;
                    }

                }
            }
            catch
            {
                Prompt.Popup("Error: Could not download update!");
            }
        }


        private AutoResetEvent _workerCompleted = new AutoResetEvent(false);
        private void metroButton3_ClickAsync(object sender, EventArgs e)
        {
            metroButton3.Enabled = false;
            try
            {
                bw.DoWork += new DoWorkEventHandler(Bw_DoWork);
                bw.RunWorkerAsync();
            }
            catch (Exception a)
            {
                Prompt.Popup("Error: Could not download update!" + Environment.NewLine + a.ToString());
            }
        }

        private async void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            await DownloadUpdate(AppPath);
            client_DownloadFileCompleted();
        }

        void client_DownloadFileCompleted()
        {
            IArchive rar = SharpCompress.Archive.Rar.RarArchive.Open(AppPath + "\\BnS Buddy [By Kogaru].rar", Options.None);
            double totalSize = rar.Entries.Where(a => !a.IsDirectory).Sum(a => a.Size);
            metroProgressBar1.Value = 100;
            long completed = 0;
            Directory.CreateDirectory(AppPath + "\\Update");
            foreach (var entry in rar.Entries.Where(a => !a.IsDirectory))
            {
                entry.WriteToDirectory(AppPath + "\\Update", ExtractOptions.Overwrite);
                completed += entry.Size;
                var percentage = completed / totalSize * 100;
                metroProgressBar1.Value = 100 + Convert.ToInt32(percentage);
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
            X509Certificate certificate = null;
            bool verified = false;
            string tmp = "";
            try
            {
                certificate = X509Certificate2.CreateFromSignedFile(AppPath + "\\BnS Buddy.exe");
                tmp = certificate.GetHashCode().ToString();
                if (tmp != "1307602086" || tmp != BnSBuddySerial) { Prompt.Popup("BnS Buddy signature does not match! Please Delete and get a fresh copy."); }
                else { verified = true; }
            }
            catch { Prompt.Popup("BnS Buddy is not signed! Please Delete and get a fresh copy."); }
            if (verified == false)
            {
                File.Delete(AppPath + "\\BnS Buddy.exe");
                Prompt.Popup("BnS Buddy.exe that was downloaded is not safe! Please contact Endless-sama via discord ASAP!");
            }
            else
            {
                Process BnSBuddy = new Process();
                BnSBuddy.StartInfo.FileName = AppPath + "\\BnS Buddy.exe";
                BnSBuddy.Start();
            }
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

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            if (Ready == true)
            {
                if (metroToggle1.Checked == true)
                {
                    if (File.ReadAllText(AppPath + "\\Settings.ini").Contains("autoupdate = false"))
                    {
                        var fileContents = System.IO.File.ReadAllText(@AppPath + "\\Settings.ini");
                        fileContents = fileContents.Replace("autoupdate = false", "autoupdate = true");
                        System.IO.File.WriteAllText(@AppPath + "\\Settings.ini", fileContents);
                    }
                }
                else
                {
                    if (File.ReadAllText(AppPath + "\\Settings.ini").Contains("autoupdate = true"))
                    {
                        var fileContents = System.IO.File.ReadAllText(@AppPath + "\\Settings.ini");
                        fileContents = fileContents.Replace("autoupdate = true", "autoupdate = false");
                        System.IO.File.WriteAllText(@AppPath + "\\Settings.ini", fileContents);
                    }
                }
            }
        }
    }
}
