using System;
using System.Windows.Forms;

namespace MenuConfig
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(Environment.GetCommandLineArgs().Length == 4)
            {
                if (Environment.GetCommandLineArgs()[1] == "CWP")
                {
                    if (System.IO.File.Exists(Environment.GetCommandLineArgs()[3]))
                        System.IO.File.Delete(Environment.GetCommandLineArgs()[3]);
                    System.IO.File.Move(Environment.GetCommandLineArgs()[2], Environment.GetCommandLineArgs()[3]);
                }

                if (Environment.GetCommandLineArgs()[1] == "INSTALL")
                {
                    bool isX64 = Environment.GetCommandLineArgs()[2] == "X64";
                    Installer.InstallToSystem(isX64);

                    if (Environment.GetCommandLineArgs()[3] != "?")
                        Installer.CreateRegConfig(Environment.GetCommandLineArgs()[3]);

                    if (!System.IO.File.Exists(Installer.GetConfigPath()))
                        Installer.DeployDefaultConfigFile();
                }

                Application.Exit();
                return;
            }

            if (Environment.GetCommandLineArgs().Length == 3)
            {
                if (Environment.GetCommandLineArgs()[1] == "STARTUP")
                {
                    if (Environment.GetCommandLineArgs()[2] == "?")
                        Installer.RemoveRegConfig();
                    else
                        Installer.CreateRegConfig(Environment.GetCommandLineArgs()[2]);
                }

                if (Environment.GetCommandLineArgs()[1] == "UNINSTALL")
                {
                    Installer.UninstallFromSystem(Environment.GetCommandLineArgs()[2] != "0");
                }

                if (Environment.GetCommandLineArgs()[1] == "EMT")
                {
                    Installer.ExplorerImmersiveMenuUsing(Environment.GetCommandLineArgs()[2] != "0");
                }

                Application.Exit();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
