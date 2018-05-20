using SharpCompress.Archive;
using SharpCompress.Common;
using SharpCompress.Compressor;
using SharpCompress.Compressor.Deflate;
using System;
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
        private WebClient client = new WebClient();

        // Background worker
        private BackgroundWorker Worker = new BackgroundWorker();

        // Bool to tell app is ready
        private bool Ready = false;

        // Cert Stuff
        private string BnSBuddySerial = "";

        private string BnSServerCert = "";

        public Form1()
        {
            /* Set Private Path */
            Prompt.AppPath = AppPath;
            /* Initialize Updater */
            InitializeComponent();
            /* Get Color */
            SetFormColor();
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
            /* Set updater as ready */
            Ready = true;
        }

        private void SetFormColor()
        {
            if (File.Exists(@AppPath + "\\Settings.ini"))
            {
                string line = File.ReadLines(@AppPath + "\\Settings.ini").Skip(43).Take(1).First().Replace("buddycolor = ", "");
                // Set style
                if (line == "Black")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Black;
                }
                else if (line == "Red")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Red;
                }
                else if (line == "Purple")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Purple;
                }
                else if (line == "Pink")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Pink;
                }
                else if (line == "Orange")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Orange;
                }
                else if (line == "Magenta")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Magenta;
                }
                else if (line == "Lime")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Lime;
                }
                else if (line == "Green")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Green;
                }
                else if (line == "Default")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Default;
                }
                else if (line == "Brown")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Brown;
                }
                else if (line == "Blue")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Blue;
                }
                else if (line == "Silver")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Silver;
                }
                else if (line == "Teal")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Teal;
                }
                else if (line == "White")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.White;
                }
                else if (line == "Yellow")
                {
                    metroStyleManager1.Style = MetroFramework.MetroColorStyle.Yellow;
                }
                // Set global
                Prompt.ColorSet = metroStyleManager1.Style;
            }
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
                Prompt.Popup("Please run as Admin");
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
                        sslStream.AuthenticateAsClient("updates.xxzer0modzxx.net", null, System.Security.Authentication.SslProtocols.Tls12, false);
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
                metroButton3.Text = "Make sure that:" + Environment.NewLine + "1: Not any anti-virus blocks the connection." + Environment.NewLine + "2: You are online to use the updater." + Environment.NewLine + "3: Check if update server status is online";
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
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var DomainCert = new RemoteCertificateValidationCallback(delegate (object snd, X509Certificate certificate, X509Chain chainLocal, SslPolicyErrors sslPolicyErrors)
                    {
                        return true; //Accept every certificate, even if it's invalid
                    });
                    using (SslStream sslStream = new SslStream(clientVAR.GetStream(), false, DomainCert))
                    {
                        string result = "";
                        sslStream.AuthenticateAsClient(server, null, System.Security.Authentication.SslProtocols.Tls12, false);
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
                catch (Exception e)
                {
                    metroLabel3.Text = string.Format("Offline: {0}", e.ToString());
                }
            }
        }

        private void GrabCurrentVersion()
        {
            try
            {
                if (File.Exists(AppPath + "\\BnS Buddy.exe"))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(AppPath + "\\BnS Buddy.exe");
                    metroLabel4.Text = versionInfo.ProductVersion;
                }
                else { metroLabel4.Text = "Not found"; }
            }
            catch
            {
                metroLabel4.Text = "Not found";
            }
        }

        private BackgroundWorker bw = new BackgroundWorker();

        private void DownloadUpdate(string rootpath)
        {
            try
            {
                // Save Edited File
                Worker = new BackgroundWorker();
                Worker.WorkerSupportsCancellation = true;
                Worker.WorkerReportsProgress = true;
                Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
                Worker.DoWork += new DoWorkEventHandler(Worker_SaveAsync);
                if (!Worker.IsBusy)
                {
                    Worker.RunWorkerAsync(rootpath);
                }
                else { Prompt.Popup("Please wait until saving is finished."); }
            }
            catch
            {
                Prompt.Popup("Error: Could not download update!");
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bw.ReportProgress(e.ProgressPercentage);
        }

        public class HttpClientDownloadWithProgress : IDisposable
        {
            private readonly string _downloadUrl;
            private readonly string _destinationFilePath;

            private HttpClient _httpClient;

            public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

            public event ProgressChangedHandler ProgressChanged;

            public HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath)
            {
                _downloadUrl = downloadUrl;
                _destinationFilePath = destinationFilePath;
            }

            public async Task StartDownload()
            {
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                };
                _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromDays(1) };
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "BnSBuddy/" + Application.ProductVersion + " (compatible;)");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await DownloadFileFromHttpResponseMessage(response);
            }

            private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                    await ProcessContentStream(totalBytes, contentStream);
            }

            private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
            {
                var totalBytesRead = 0L;
                var readCount = 0L;
                var buffer = new byte[128];
                var isMoreToRead = true;

                using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 128, true))
                {
                    do
                    {
                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            isMoreToRead = false;
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                            continue;
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        totalBytesRead += bytesRead;
                        readCount += 1;

                        if (readCount % 100 == 0)
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                    }
                    while (isMoreToRead);
                }
            }

            private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
            {
                if (ProgressChanged == null)
                    return;

                double? progressPercentage = null;
                if (totalDownloadSize.HasValue)
                    progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

                ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
            }

            public void Dispose()
            {
                _httpClient?.Dispose();
            }
        }

        private async void Worker_SaveAsync(object senderer, DoWorkEventArgs e)
        {
            string rootpath = e.Argument.ToString();
            using (var client = new HttpClientDownloadWithProgress("https://updates.xxzer0modzxx.net/BnS%20Buddy%20[By%20Kogaru].rar", rootpath + "\\BnS Buddy [By Kogaru].rar"))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    metroProgressBar1.HideProgressText = false;
                    metroProgressBar1.Value = Convert.ToInt32(progressPercentage);
                    metroProgressBar1.ProgressBarStyle = ProgressBarStyle.Continuous;
                    metroProgressBar1.Refresh();
                };
                await client.StartDownload();
                _workerCompleted.Set();
            }
        }

        private AutoResetEvent _workerCompleted = new AutoResetEvent(false);

        private void metroButton3_ClickAsync(object sender, EventArgs e)
        {
            metroButton3.Enabled = false;
            metroProgressBar1.ProgressBarStyle = ProgressBarStyle.Marquee;
            metroProgressBar1.Refresh();
            try
            {
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler(Bw_DoWork);
                bw.RunWorkerAsync();
            }
            catch
            {
                Prompt.Popup("Error: Could not download update! Try again :C ");
            }
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            DownloadUpdate(AppPath);
            _workerCompleted.WaitOne();
            client_DownloadFileCompleted();
        }

        private void client_DownloadFileCompleted()
        {
            IArchive rar = SharpCompress.Archive.Rar.RarArchive.Open(AppPath + "\\BnS Buddy [By Kogaru].rar", Options.None);
            double totalSize = rar.Entries.Where(a => !a.IsDirectory).Sum(a => a.Size);
            long completed = 0;
            Directory.CreateDirectory(AppPath + "\\Update");
            foreach (var entry in rar.Entries.Where(a => !a.IsDirectory))
            {
                entry.WriteToDirectory(AppPath + "\\Update", ExtractOptions.Overwrite);
                completed += entry.Size;
                var percentage = completed / totalSize * 100;
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
            catch (Exception e)
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
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + "timeout 2 && \"" + AppPath + "\\BnS Buddy.exe\"")
                {
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process proc = new Process())
                {
                    proc.StartInfo = procStartInfo;
                    proc.Start();
                }
                //Process BnSBuddy = new Process();
                //BnSBuddy.StartInfo.FileName = AppPath + "\\BnS Buddy.exe";
                //BnSBuddy.Start();
            }
        }

        public static class Prompt
        {
            public static string AppPath { get; internal set; }

            public static MetroFramework.MetroColorStyle ColorSet { get; internal set; }

            public static void Popup(string Message)
            {
                // Get Color
                string line = File.ReadLines(@AppPath + "\\Settings.ini").Skip(43).Take(1).First().Replace("buddycolor = ", "");
                // Continue
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
                // Set style
                prompt.Style = ColorSet;
                // Prompt
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