using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Azazel.KeyHookup {
    public class WPFHotkeyManager : IDisposable {
        public delegate void HotkeyPressedEvent(Window window, Hotkey hotkey);

        private readonly Window actualWindow;

        private IntPtr handle;
        private Dictionary<int, Hotkey> hotkeys;
        private int id;

        private WindowInteropHelper window;

        public WPFHotkeyManager(Window topLevelWindow) {
            actualWindow = topLevelWindow;

            if (!actualWindow.IsArrangeValid)
                actualWindow.SourceInitialized += OnSourceInitialized;
            else OnSourceInitialized(actualWindow, null);
        }

        private ObservableCollection<Hotkey> ObservableHotkeys { get; set; }

        public void Dispose() {
            Clear();
            handle = IntPtr.Zero;
        }

        public event HotkeyPressedEvent HotkeyPressed;

        private void OnSourceInitialized(object sender, EventArgs e) {
            window = new WindowInteropHelper(actualWindow);
            handle = window.Handle;
            HwndSource.FromHwnd(handle).AddHook(WndProc);
            hotkeys = new Dictionary<int, Hotkey>();
            ObservableHotkeys = new ObservableCollection<Hotkey>();
        }

        public void Register(Hotkey key) {
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("You can't register hotkeys until your Window is loaded.");
            if (NativeMethods.RegisterHotKey(handle, ++id, key.modifiers.ModifierKeys, key.key)) {
                key.id = id;
                hotkeys.Add(id, key);
                ObservableHotkeys.Add(key);
            }
            else Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }

        public void Unregister(Hotkey key) {
            int index = IndexOf(key);
            if (index > 0) {
                if (NativeMethods.UnregisterHotKey(handle, index)) {
                    hotkeys.Remove(index);
                    ObservableHotkeys.Remove(key);
                }
                else Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

        private void Clear() {
            foreach (var key in hotkeys)
                NativeMethods.UnregisterHotKey(handle, key.Key);
            hotkeys.Clear();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int hotkey = 0x312;
            if (hotkey == msg) {
                if (hotkeys.ContainsKey(wParam.ToInt32())) {
                    HotkeyPressed(actualWindow, hotkeys[wParam.ToInt32()]);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private int IndexOf(Hotkey item) {
            if (item.id > 0 && hotkeys.ContainsKey(item.id))
                return item.id;
            if (hotkeys.ContainsValue(item)) {
                foreach (var k in hotkeys) {
                    if (item.Equals(k.Value)) {
                        item.id = k.Value.id;
                        return item.id;
                    }
                }
            }

            throw new ArgumentOutOfRangeException(string.Format("The hotkey \"{0}\" is not in this hotkey manager.", item));
        }
    }
}