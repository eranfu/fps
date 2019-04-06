using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

namespace Utils
{
    public static class WindowsUtil
    {
        [DllImport("user32.dll")]
        private static extern void EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern void SetWindowPos(
            IntPtr hWnd, int hWndInsertAfter, int x, int y, int sizeX, int sizeY, int wFlags);

        public static void SetWindowPosition(int x, int y, int sizeX = 0, int sizeY = 0)
        {
            Process process = Process.GetCurrentProcess();
            process.Refresh();

            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out int id);
                if (id == process.Id)
                {
                    SetWindowPos(hWnd, 0, x, y, sizeX, sizeY, sizeX * sizeY == 0 ? 1 : 0);
                    return false;
                }

                return true;
            }, IntPtr.Zero);
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}

#endif