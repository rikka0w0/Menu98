using System;
using System.Diagnostics;
using System.Windows.Forms;
using WinUtils;

namespace MenuConfig
{
    public static class Installer
    {
        public const String installedMenu98Dll = "Menu98.dll";
        public const String installedTMTDll = "TaskbarContextMenuTweaker.dll";

        public static string GetMenu98Path()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), installedMenu98Dll);
        }

        public static string GetTMTPath()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), installedTMTDll);
        }

        public static string GetConfigPath()
        {
            return System.IO.Path.Combine(System.IO.Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName, "Menu.xml");
        }

        public static void CreateRegConfig(string InjectParam)
        {
            Microsoft.Win32.Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", "TMT", "rundll32 " + installedTMTDll + ", Inject", Microsoft.Win32.RegistryValueKind.String);
        }

        public static void RemoveRegConfig()
        {
            Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).DeleteValue("TMT", false);
        }

        public static void UninstallFromSystem(bool delConfigFile)
        {
            RemoveRegConfig();
            if (System.IO.File.Exists(GetMenu98Path()))
                System.IO.File.Delete(GetMenu98Path());
            if (System.IO.File.Exists(GetTMTPath()))
                System.IO.File.Delete(GetTMTPath());
            if (delConfigFile && System.IO.File.Exists(GetConfigPath()))
                System.IO.File.Delete(GetConfigPath());
        }

        private static void UninstallWithPrivilage(bool deleteConfig)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Arguments = "UNINSTALL " + (deleteConfig ? "1" : "0");
            startInfo.Verb = "runas";

            try
            {
                Process.Start(startInfo).WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This action requires administrative privilage!", "Fail to uninstall", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LaunchUninstall(Form Parent)
        {
            bool deleteConfig = false;
            if (WinVer.IsVistaOrGreater())
            {
                TaskDialog dlg = new TaskDialog();
                dlg.Icon = Properties.Resources.Installer.Handle;
                dlg.WindowTitle = "Uninstall";
                dlg.MainInstruction = "Uninstall this software from your system";
                dlg.Content = "This program will not be deleted.";
                dlg.AddButton((int)DialogResult.Yes, "Uninstall now").ShowShield = true;
                dlg.CommonButtons = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CANCEL_BUTTON;
                dlg.UseCommandLinks = true;
                dlg.CheckBoxText = "Delete configuration file";
                if (dlg.ShowDialog(Parent.Handle) == (int)DialogResult.Yes)
                    deleteConfig = dlg.CheckBoxChecked;
                else
                    return;
            }
            else
            {
                switch(MessageBox.Show(Parent, "Uninstall this software from your system.\nYes - Uninstall without deleting config file\nNo - Uninstall and delete the config file", "Uninstall", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        deleteConfig = false;
                        break;
                    case DialogResult.No:
                        deleteConfig = true;
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }

            if (MenuControl.isMenu98ModuleLoaded())
                MenuControl.Menu98_ExitHost();

            try
            {
                UninstallFromSystem(deleteConfig);
            }
            catch (UnauthorizedAccessException)
            {
                UninstallWithPrivilage(deleteConfig);
            }
            catch (System.Security.SecurityException)
            {
                UninstallWithPrivilage(deleteConfig);
            }

            MessageBox.Show(Parent, "Uninstalled successfully", "Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        public static void InstallToSystem(bool isX64)
        {
            System.IO.File.WriteAllBytes(GetMenu98Path(), isX64 ? Properties.Resources.Menu98_x64 : Properties.Resources.Menu98_x86);
            System.IO.File.WriteAllBytes(GetTMTPath(), isX64 ? Properties.Resources.TMT_x64 : Properties.Resources.TMT_x86);
        }

        public static void ShowInstaller(Form Parent)
        {
            string param = null;
            if (WinVer.IsVistaOrGreater())
            {
                TaskDialog dlg = new TaskDialog();
                dlg.Icon = Properties.Resources.Installer.Handle;
                dlg.WindowTitle = "Install TMT & Menu98";
                dlg.MainInstruction = "Install TMT & Menu98";
                dlg.Content = "This tweak requires <a href=\"https://www.microsoft.com/en-us/download/details.aspx?id=49984\">VC2015 runtime</a> installed!";
                dlg.ExpandedControlText = "Start up parameter:";
                dlg.OnRadioButtonSelected += InstallTD_OnRadioButtonSelected;
                dlg.OnHyperLinkClicked += InstallTD_OnHyperLinkClicked;

                for (int id = 0; id <= MenuControl.OSList.Length; id++)
                {
                    dlg.AddRadioButton(id, MenuControl.GetOSFromIndex(id));
                }

                dlg.SelectedRadioButton = MenuControl.Os2ListIndex(WinVer.SystemVersion);

                param = MenuControl.GetInjectParamFromIndex(dlg.SelectedRadioButton);
                if (param == null)
                    dlg.ExpandedInformation = "No startup";
                else
                    dlg.ExpandedInformation = param;
                dlg.ExpandFooterAreaByDefault = true;

                dlg.AddButton(100, "Install Now").ShowShield = true;

                dlg.CommonButtons = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CLOSE_BUTTON;
                dlg.UseCommandLinks = true;
                dlg.EnableHyperLinks = true;

                int ret = dlg.ShowDialog(Parent.Handle);

                if (ret != 100)
                    return;

                param = MenuControl.GetInjectParamFromIndex(dlg.SelectedRadioButton);
            }
            else
            {
                switch (MessageBox.Show(Parent, "Yes - Install and auto startup\nNo - Just install", "Install Menu98", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                {
                    case DialogResult.Yes:
                        param = "!Button"; //Hard coding here
                        break;
                    case DialogResult.No:
                        param = null; //Hard coding here
                        break;
                    default:
                        return;
                }
            }


            try
            {
                InstallToSystem(WinVer.IsX64System());

                if (param == null)
                    CreateRegConfig(param);

                if (!System.IO.File.Exists(GetConfigPath()))
                    DeployDefaultConfigFile();
            }
            catch (UnauthorizedAccessException)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Arguments = "INSTALL X" + (WinVer.IsX64System() ? "64" : "86") + " ";


                if (param == null)
                {
                    startInfo.Arguments += "\"?\"";
                }
                else
                {
                    startInfo.Arguments += "\"" + param + "\"";
                }

                startInfo.Verb = "runas";

                try
                {
                    System.Diagnostics.Process.Start(startInfo).WaitForExit();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("This action requires administrative privilage!", "Fail to install", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

                

            if (param != null)
                MenuControl.Menu98_Inject(param);
        }

        private static void InstallTD_OnHyperLinkClicked(TaskDialog sender, object UserData, string url)
        {
            Process.Start(url);
        }

        private static void InstallTD_OnRadioButtonSelected(TaskDialog sender, object UserData, int ID)
        {
            string startParam = MenuControl.GetInjectParamFromIndex(sender.SelectedRadioButton);
            if (startParam == null)
            {
                sender.ExpandedInformation = "No startup";
            }
            else
            {
                sender.ExpandedInformation = "Startup command: rundll32.exe " + Installer.installedTMTDll +", Inject " + startParam;
            }
        }

        public static DialogResult ShowSelectConfigFileBox(Form Parent)
        {
            if (WinVer.IsVistaOrGreater())
            {
                TaskDialog dlg = new TaskDialog();
                dlg.SetIcon(TASKDIALOG_ICON.TD_INFORMATION_ICON);
                dlg.WindowTitle = "Open configuration file";
                dlg.MainInstruction = "You will see this dialog if the configuration file (menu.xml) is missing from your system";
                dlg.AddButton((int)DialogResult.Yes, "Create a config file").ShowShield = true;
                dlg.AddButton((int)DialogResult.No, "Open an existing config file");
                dlg.CommonButtons = TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_CANCEL_BUTTON;
                dlg.UseCommandLinks = true;
                return (DialogResult)dlg.ShowDialog(Parent.Handle);
            }
            else
            {
                return MessageBox.Show(Parent, "You will see this dialog if the configuration file (menu.xml) is missing from your system\n\nYes - Create a config file\nNo - Open an existing config file\nCancel - Quit", "Open configuration file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            }
        }

        public static void DeployDefaultConfigFile()
        {
            try
            {
                System.IO.File.WriteAllText(GetConfigPath(), Properties.Resources.defaultConfig);
            }
            catch (UnauthorizedAccessException)
            {
                string tmp = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllText(tmp, Properties.Resources.defaultConfig);


                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Arguments = "CWP \"" + tmp + "\" \"" + GetConfigPath() + "\"";
                startInfo.Verb = "runas";

                try
                {
                    System.Diagnostics.Process.Start(startInfo).WaitForExit();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("This action requires administrative privilage!", "Fail to create the config file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }  

        public static bool ExplorerImmersiveMenuRemoved()
        {
            object val = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\FlightedFeatures", "ImmersiveContextMenu", null);
            if (val == null)
                return false;
            if ((int)val == 0)
                return true;  
            return false;
        }

        private static void ChangeExplorerImmersiveMenuWithPrivilage(bool val)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Arguments = "EMT " + (val ? "1" : "0");
            startInfo.Verb = "runas";

            try
            {
                Process.Start(startInfo).WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This action requires administrative privilage!", "Fail to change the setting", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ExplorerImmersiveMenuUsing(bool val)
        {
            try
            {
                if (val)
                {
                    Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\FlightedFeatures", true).DeleteValue("ImmersiveContextMenu", false);
                }
                else
                {
                    Microsoft.Win32.Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\FlightedFeatures", "ImmersiveContextMenu", 0, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
            catch (System.Security.SecurityException)
            {
                ChangeExplorerImmersiveMenuWithPrivilage(val); 
            }
            catch (UnauthorizedAccessException)
            {
                ChangeExplorerImmersiveMenuWithPrivilage(val);
            }

        }
    }
}