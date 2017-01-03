using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopNote
{
    class SingleInstance
    {
        public static readonly IntPtr HWND_BROADCAST = (IntPtr)0xffff;
        public static readonly int RegisteredMsg = RegisterWindowMessage("WM_SHOW_DESKTOPNOTE");
        //[DllImport("user32")] //not working when ShowInTaskbar set to false.
        //public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SendNotifyMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string message);

        public enum MutexScope { Local, Global }

        /// <summary>
        /// Return created mutex if the mutex name does not exist. Returns null if the mutex name exists.
        /// </summary>
        public static bool CheckExist(string uniquestr, MutexScope scope = MutexScope.Global)
        {sdrsfsdf
            bool createdNew;
            new Mutex(true, scope.ToString() + @"\" + uniquestr, out createdNew);
            return !createdNew;
        }
    }
}
