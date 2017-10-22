using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace DesktopNote
{
    class WinAPI
    {
        #region ToolWindow
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int value);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr window, int index);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;

        public static int SetToolWindow(Window win)
        {
            var handle = new WindowInteropHelper(win).Handle;
            return SetWindowLong(handle, GWL_EXSTYLE, GetWindowLong(handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
        }
        #endregion

        #region WindowPosition
        public static IntPtr
            HWND_NOTOPMOST = new IntPtr(-2),
            HWND_TOPMOST = new IntPtr(-1),
            HWND_TOP = new IntPtr(0),
            HWND_BOTTOM = new IntPtr(1);

        public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        public static bool BringToTop(Window win)
        {
            var handle = new WindowInteropHelper(win).Handle;
            return SetWindowPos(handle, HWND_TOP, 0, 0, 0, 0, NOACTIVATE | NOMOVE | NOSIZE);
        }
        #endregion
    }
}
