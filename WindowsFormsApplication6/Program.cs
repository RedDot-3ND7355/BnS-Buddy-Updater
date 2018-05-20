using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication6
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///
        private static Mutex mutex = null;
        public static string AppPath = Path.GetDirectoryName(Application.ExecutablePath);

        [STAThread]
        private static void Main()
        {
            const string appName = "BnSBuddy";
            mutex = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                Prompt.Popup("Please Kill Related BnS Buddy Process to proceed!");
                return;
            }
            string resource0 = "WindowsFormsApplication6.MetroFramework.Design.dll";
            EmbeddedAssembly.Load(resource0, "MetroFramework.Design.dll");
            string resource1 = "WindowsFormsApplication6.MetroFramework.dll";
            EmbeddedAssembly.Load(resource1, "MetroFramework.dll");
            string resource2 = "WindowsFormsApplication6.MetroFramework.Fonts.dll";
            EmbeddedAssembly.Load(resource2, "MetroFramework.Fonts.dll");
            string resource3 = "WindowsFormsApplication6.SharpCompress.dll";
            EmbeddedAssembly.Load(resource3, "SharpCompress.dll");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Preloader());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        public static class Prompt
        {
            public static string AppPath { get; internal set; }

            public static void Popup(string Message)
            {
                string line = "Default";
                // Get Color
                if (File.Exists(@AppPath + "\\Settings.ini"))
                {
                    line = File.ReadLines(@AppPath + "\\Settings.ini").Skip(43).Take(1).First().Replace("buddycolor = ", "");
                }
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
                    Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon"))),
                    ControlBox = false,
                    Theme = MetroFramework.MetroThemeStyle.Dark,
                    Style = MetroFramework.MetroColorStyle.Black,
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
                if (line == "Black")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Black;
                }
                else if (line == "Red")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Red;
                }
                else if (line == "Purple")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Purple;
                }
                else if (line == "Pink")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Pink;
                }
                else if (line == "Orange")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Orange;
                }
                else if (line == "Magenta")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Magenta;
                }
                else if (line == "Lime")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Lime;
                }
                else if (line == "Green")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Green;
                }
                else if (line == "Default")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Default;
                }
                else if (line == "Brown")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Brown;
                }
                else if (line == "Blue")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Blue;
                }
                else if (line == "Silver")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Silver;
                }
                else if (line == "Teal")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Teal;
                }
                else if (line == "White")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.White;
                }
                else if (line == "Yellow")
                {
                    prompt.Style = MetroFramework.MetroColorStyle.Yellow;
                }
                // Prompt
                prompt.ShowDialog();
            }
        }
    }
}