using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Azazel.KeyHookup {
    [Serializable]
    public struct Hotkey {
        internal int id;
        public readonly Keys key;
        public readonly Modifiers modifiers;

        public Hotkey(Modifiers modifiers, Keys key) {
            this.modifiers = modifiers;
            this.key = key;
            id = -1;
        }

        public static Hotkey Parse(string key) {
            var rx = new Regex(@"(?:(?:(?<win>win)|(?<ctrl>ctrl|control|ctl)|(?<alt>alt)|(?<shift>shift))\s*(?:[\|+-])\s*)+(?<key>.*)",
                               RegexOptions.IgnoreCase);

            var mods = ModifierKeys.None;

            var m = rx.Match(key);

            if (!m.Success) return new Hotkey();

            if (m.Groups["win"].Success)
                mods |= ModifierKeys.Windows;
            if (m.Groups["ctrl"].Success)
                mods |= ModifierKeys.Control;
            if (m.Groups["alt"].Success)
                mods |= ModifierKeys.Alt;
            if (m.Groups["shift"].Success)
                mods |= ModifierKeys.Shift;

            return new Hotkey(new Modifiers(mods), (Keys) Enum.Parse(typeof (Keys), m.Groups["key"].Value, true));
        }

        public override string ToString() {
            var hotKeyString = new StringBuilder();
            if (modifiers.HasWin)
                hotKeyString.Append("WIN + ");
            if (modifiers.HasCtrl)
                hotKeyString.Append("CTRL + ");
            if (modifiers.HasAlt)
                hotKeyString.Append("ALT + ");
            if (modifiers.HasShift)
                hotKeyString.Append("SHIFT + ");
            return hotKeyString.Append(key).ToString();
        }

        public bool Equals(Hotkey hotkey) {
            return Equals(key, hotkey.key) && Equals(modifiers, hotkey.modifiers);
        }

        public override bool Equals(object obj) {
            if (!(obj is Hotkey)) return false;
            return Equals((Hotkey) obj);
        }

        public override int GetHashCode() {
            return key.GetHashCode() + 29*modifiers.GetHashCode();
        }
    }

    public class WPFHotkeyManager : IDisposable {
        public delegate void HotkeyPressedEvent(Window window, Hotkey hotkey);

        private readonly Window actualWindow;

        private IntPtr handle;
        private Dictionary<int, Hotkey> Hotkeys;
        private int id;
        private ObservableCollection<Hotkey> observableHotkeys;

        private WindowInteropHelper window;

        public WPFHotkeyManager(Window TopLevelWindow) {
            actualWindow = TopLevelWindow;

            if (!actualWindow.IsArrangeValid)
                actualWindow.SourceInitialized += OnSourceInitialized;
            else OnSourceInitialized(actualWindow, null);
        }

        public ReadOnlyCollection<Hotkey> ReadOnlyHotkeys {
            get {
                var hotkeys = new Hotkey[Hotkeys.Count];
                Hotkeys.Values.CopyTo(hotkeys, 0);
                return new ReadOnlyCollection<Hotkey>(hotkeys);
            }
        }

        public ObservableCollection<Hotkey> ObservableHotkeys {
            get { return observableHotkeys; }
        }

        public int Count {
            get { return Hotkeys.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public Hotkey this[int index] {
            get { return Hotkeys[index]; }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public void Dispose() {
            Clear();
            handle = IntPtr.Zero;
        }

        public event HotkeyPressedEvent HotkeyPressed;

        private void OnSourceInitialized(object sender, EventArgs e) {
            window = new WindowInteropHelper(actualWindow);
            handle = window.Handle;
            HwndSource.FromHwnd(handle).AddHook(WndProc);
            Hotkeys = new Dictionary<int, Hotkey>();
            observableHotkeys = new ObservableCollection<Hotkey>();
        }

        public Hotkey GetKey(int Id) {
            return Hotkeys[Id];
        }

        public void Register(Hotkey key) {
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("You can't register hotkeys until your Window is loaded.");
            if (NativeMethods.RegisterHotKey(handle, ++id, key.modifiers.ModifierKeys, key.key)) {
                key.id = id;
                Hotkeys.Add(id, key);
                observableHotkeys.Add(key);
            }
            else Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }

        public void Unregister(Hotkey key) {
            var index = IndexOf(key);
            if (index > 0) {
                if (NativeMethods.UnregisterHotKey(handle, index)) {
                    Hotkeys.Remove(index);
                    observableHotkeys.Remove(key);
                }
                else Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

        private void Clear() {
            foreach (var key in Hotkeys)
                NativeMethods.UnregisterHotKey(handle, key.Key);
            Hotkeys.Clear();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (WM.Hotkey == msg) {
                if (Hotkeys.ContainsKey(wParam.ToInt32())) {
                    HotkeyPressed(actualWindow, Hotkeys[wParam.ToInt32()]);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Add(Hotkey item) {
            if (Hotkeys.ContainsValue(item))
                throw new ArgumentException("That Hotkey is already registered.");
            Register(item);
        }

        public bool Contains(Hotkey item) {
            return Hotkeys.ContainsValue(item);
        }

        public void CopyTo(Hotkey[] array, int arrayIndex) {
            Hotkeys.Values.CopyTo(array, arrayIndex);
        }

        private void Remove(Hotkey item) {
            Unregister(item);
        }

        public IEnumerator<Hotkey> GetEnumerator() {
            return Hotkeys.Values.GetEnumerator();
        }

        private int IndexOf(Hotkey item) {
            if (item.id > 0 && Hotkeys.ContainsKey(item.id))
                return item.id;
            if (Hotkeys.ContainsValue(item)) {
                foreach (var k in Hotkeys) {
                    if (item.Equals(k.Value)) {
                        item.id = k.Value.id;
                        return item.id;
                    }
                }
            }

            throw new ArgumentOutOfRangeException(string.Format("The hotkey \"{0}\" is not in this hotkey manager.", item));
        }

        public void RemoveAt(int index) {
            Remove(Hotkeys[index]);
        }
    }

    public struct WM {
        internal const int GetHotkey = 0x33;
        internal const int Hotkey = 0x312;
        internal const int SetHotkey = 0x0032;
    }

    public static class NativeMethods {
        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys uVirtKey);

        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}