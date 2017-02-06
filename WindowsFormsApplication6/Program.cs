using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication6
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
            Application.Run(new Form1());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
    }
}
