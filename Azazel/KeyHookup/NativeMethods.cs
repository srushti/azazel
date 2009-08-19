using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace Azazel.KeyHookup {
    public static class NativeMethods {
        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys uVirtKey);

        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}