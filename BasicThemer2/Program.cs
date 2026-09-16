using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicThemer2
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize localization before any UI text is shown
            global::BasicThemer2.BasicThemer2.InitLanguageSetting();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Any(x => x.Contains("help")) || args.Any(x => x.Contains("?")))
            {
                MessageBox.Show(Strings.CmdHelp, Strings.CmdHelpTitle);
                return;
            }

            if (args.Any(x => x.Contains("ver")))
            {
                MessageBox.Show(string.Format(Strings.CmdVersion, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion), Strings.AppName);
                return;
            }

            // Single-instance guard.
            using (var instanceMutex = new Mutex(true, "Local\\BasicThemer2", out bool createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(Strings.MsgAlreadyRunning, Strings.AppName);
                    return;
                }

                if (!IsAdministrator() && !args.Any(x => x.Contains("noadminalert")))
                {
                    if (MessageBox.Show(Strings.AdminPrompt, Strings.AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RerunAsAdministrator();
                        MessageBox.Show(Strings.AdminAborted, Strings.AppName);
                    }
                }

                Application.Run(new BasicThemer2());
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RerunAsAdministrator()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
                args = args.Skip(1).ToArray();

            string argString = string.Empty;
            foreach (string s in args)
                argString = argString + "\"" + s + "\" ";

            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            try
            {
                Process.Start(new ProcessStartInfo(exeName, argString)
                {
                    Verb = "runas"
                });
                Process.GetCurrentProcess().Kill();
            }
            catch (Win32Exception) { }
        }
    }
}
