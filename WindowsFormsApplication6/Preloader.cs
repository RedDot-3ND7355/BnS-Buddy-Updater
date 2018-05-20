using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using WindowsFormsApplication6.Properties;

namespace WindowsFormsApplication6
{
    public partial class Preloader : Form
    {
        public string AppPath = Path.GetDirectoryName(Application.ExecutablePath);
        public bool AdminCheck = true;

        public Preloader()
        {
            // Do File check and attempt creating if missing
            if (!File.Exists(AppPath + "\\MetroFramework.dll"))
            {
                try
                {
                    // Generate missing dependency & run
                    File.WriteAllBytes(AppPath + "\\MetroFramework.dll", Resources.MetroFramework);
                }
                catch { MessageBox.Show("Failed to generate MetroFramework.dll, try running as admin."); KillApp(); }
            }
            // Run
            Application.Run(new Form1());
            // Dispose current thread
            Dispose();
        }

        public void KillApp()
        {
            Process p = Process.GetCurrentProcess();
            p.Kill();
        }
    }
}