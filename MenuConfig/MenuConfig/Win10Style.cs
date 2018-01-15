using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace Aero
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    class Win10Style
    {
        [DllImport("dwmapi.dll")]
        public static extern void DwmIsCompositionEnabled(ref int enabledptr);
        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margin);



        [DllImport("User32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)]bool bRevert);
        [DllImport("User32.dll")]
        public static extern int EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        public const uint SC_CLOSE = 0xF060;
        public const uint SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern IntPtr TrackPopupMenuEx(IntPtr hMenu, uint un, uint n1, uint n2, IntPtr hWnd, IntPtr lpTPMParams);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CallingConvention = CallingConvention.Winapi)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern long GetCursorPos(ref Point lpPoint);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;






        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        public struct MARGINS
        {
            public int m_Left;
            public int m_Right;
            public int m_Top;
            public int m_Buttom;
        };


        public static Color GetAeroColor()
        {
            return Color.FromArgb(164, 212, 211);
        }

        //Win10专用
        public static void EnableBlur(IntPtr HWnd)
        {

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            accent.GradientColor = 0xFF0000;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(HWnd, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        //Vista之后通用
        public static void EnableAero(Form target)
        {
            MARGINS mg = new MARGINS();
            mg.m_Buttom = -1;
            mg.m_Left = -1;
            mg.m_Right = -1;
            mg.m_Top = -1;

            DwmExtendFrameIntoClientArea(target.Handle, ref mg);
            
            target.BackColor = GetAeroColor();
            target.TransparencyKey = GetAeroColor();
        }

        public static void DragWindow(IntPtr hwnd)
        {
            ReleaseCapture();
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        public static void ShowSystemMenu(IntPtr hwnd)
        {
            const uint TPM_RETURNCMD = 0x0100;
            const uint TPM_LEFTBUTTON = 0x0;

            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            Point defPnt = new Point();
            GetCursorPos(ref defPnt);

            IntPtr hmenu = GetSystemMenu(hwnd, false);

            IntPtr cmd = TrackPopupMenuEx(hmenu, TPM_LEFTBUTTON | TPM_RETURNCMD, (uint)defPnt.X, (uint)defPnt.Y, hwnd, IntPtr.Zero);
            if (cmd != IntPtr.Zero)
            {
                PostMessage(hwnd, WM_SYSCOMMAND, cmd, IntPtr.Zero);
            }
        }

        public static void ShowShield(IntPtr buttonHandle, bool showShield)
        {
            SendMessage(buttonHandle, 0x160c, 0, showShield ? 1 : 0);
        }
    }
}
