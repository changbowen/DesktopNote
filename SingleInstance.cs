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
        //from MSDN: If two different applications register the same message string, the applications return the same message value.
        //The message remains registered until the session ends.
        public static readonly int RegisteredWM = RegisterWindowMessage("WM_SHOW_DESKTOPNOTE");
        //[DllImport("user32")] //not working when ShowInTaskbar set to false.
        //public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SendNotifyMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string message);

        public enum MutexScope { Local, Global }

        /// <summary>
        /// Return false and the created mutex if the mutex name does not exist. Otherwise returns true and null.
        /// </summary>
        public static bool CheckExist(string uniquestr, ref Mutex mtx, MutexScope scope = MutexScope.Global)
        {
            bool createdNew;
            var newmtx = new Mutex(false, scope.ToString() + @"\" + uniquestr, out createdNew);
            if (createdNew)
                mtx = newmtx;
            else
                newmtx.Close();
            return !createdNew;
        }
    }
}
