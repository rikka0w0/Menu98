using System;
using System.Diagnostics;
using WinUtils;

namespace MenuConfig
{


    public static class MenuControl
    {
        const  int WM_MENU98 = 0x0409;
        const int MENU98_EXITHOST = 0x90;

        public static string[] OSList = {
            "Windows 10/2016",
            "Windows 8.1/2012",
            "Windows 7/2008 r2",
            "Windows Vista/2008",
            "Windows XP/2003",
        };

        public static string[] InjectParamList =
        {
            "!Start",
            "!Start",
            "Button",
            "Button",
            "!Button"
        };

        public static string GetOSFromIndex(int i)
        {
            if (i == OSList.Length)
                return "Others";
            else
                return OSList[i];
        }

        public static string GetInjectParamFromIndex(int i)
        {
            if (i == InjectParamList.Length)
                return null;
            else
                return InjectParamList[i];
        }

        public static int Os2ListIndex(Version ver)
        {
            switch (ver.Major)
            {
                case 10:    //Windows 10
                    return 0;
                case 6:
                    if (ver.Minor == 3) //Windows 8.1
                        return 1;
                    else if (ver.Minor == 2) //Windows 8
                        return OSList.Length;
                    else if (ver.Minor == 1) //Windows 7
                        return 2;
                    else if (ver.Minor == 0) //Windows Vista
                        return 3;
                    else
                        return OSList.Length;
                case 5:     //Windows XP
                    return 4;   
                default:
                    return OSList.Length;
            }
        }

        public static bool isMenu98ModuleLoaded()
        {
            int pid;
            WinAPI.GetWindowThreadProcessId(WinAPI.FindWindow("Shell_TrayWnd", null), out pid);
            Process p = Process.GetProcessById(pid);

            foreach (ProcessModule m in p.Modules)
            {
                if (m.ModuleName.ToLower() == Installer.installedTMTDll.ToLower())
                    return true;
            }
            return false;
        }

        public static void Menu98_Inject(string InjectParam)
        {
            Process.Start("rundll32.exe", Installer.installedTMTDll + ", Inject " + InjectParam).WaitForExit();
        }

        public static void Menu98_ExitHost()
        {
            IntPtr hWnd = WinAPI.FindWindow("Shell_TrayWnd", null);
            WinAPI.SendMessage(hWnd, WM_MENU98, (IntPtr)MENU98_EXITHOST, IntPtr.Zero);
        }
    }
}