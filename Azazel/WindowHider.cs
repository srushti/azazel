using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Azazel {
    public class WindowHider {
        private readonly Window window;
        private const int GWL_EXSTYLE = (-20);
        private const int WS_EX_APPWINDOW = 0x40000;
        private const int WS_EX_TOOLWINDOW = 0x80;

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public WindowHider(Window window) {
            this.window = window;
        }

        public void Hide() {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            SetWindowLong(handle, GWL_EXSTYLE, (GetWindowLong(handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
        }
    }
}