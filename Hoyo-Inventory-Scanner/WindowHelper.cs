using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Hoyo_Inventory_Scanner
{
    public static class WindowHelper {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool FocusWindow(string processName) {
            var process = Process.GetProcessesByName(processName).FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

            if(process == null)
                return false;

            IntPtr handle = process.MainWindowHandle;

            ShowWindow(handle, SW_RESTORE);

            return SetForegroundWindow(handle);
        }

        public static Point GetTopLeft(string windowTitle) {
            IntPtr hWnd = FindWindow(null, windowTitle);
            if (hWnd == IntPtr.Zero)
                Logger.LogDebug("GetTopLeft() : window not found");

            if (GetWindowRect(hWnd, out RECT rect))
                return new Point(rect.Left, rect.Top);

            return new Point(0, 0);
        }
    }
}
