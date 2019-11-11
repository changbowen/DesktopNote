using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace DesktopNote
{
    class GetCurrentMonitor
    {
        private const int MONITOR_DEFAULTTOPRIMERTY = 0x1;
        private const int MONITOR_DEFAULTTONEAREST = 0x2;
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hwnd, ref MonitorInfo mInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public uint cbSize;
            public Rect2 rcMonitor;
            public Rect2 rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect2
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public Rect GetInfo(Window win)
        {
            if (!win.IsLoaded || PresentationSource.FromVisual(win) == null) return new Rect();

            var mi = new MonitorInfo();
            mi.cbSize = (uint)Marshal.SizeOf(typeof(MonitorInfo));
            var hwmon = MonitorFromWindow(new System.Windows.Interop.WindowInteropHelper(win).EnsureHandle(), MONITOR_DEFAULTTOPRIMERTY);
            if (GetMonitorInfo(hwmon, ref mi))
            {
                //convert to device-independent vaues
                var mon = mi.rcMonitor;
                Point realp1;
                Point realp2;
                var trans = PresentationSource.FromVisual(win).CompositionTarget.TransformFromDevice;
                realp1 = trans.Transform(new Point(mon.left, mon.top));
                realp2 = trans.Transform(new Point(mon.right, mon.bottom));
                return new Rect(realp1, realp2);
            }
            else
                throw new Exception("Failed to get monitor info.");
        }
    }
}
