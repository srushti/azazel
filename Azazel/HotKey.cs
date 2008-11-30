using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Azazel {
    [Serializable]
    public struct Hotkey {
        internal int id;
        public Keys key;
        public ModifierKeys modifiers;

        public Hotkey(ModifierKeys modifiers, Keys key) {
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

            return new Hotkey(mods, (Keys) Enum.Parse(typeof (Keys), m.Groups["key"].Value, true));
        }

        public override string ToString() {
            var hotKeyString = new StringBuilder();
            if ((ModifierKeys.Windows & modifiers) == ModifierKeys.Windows)
                hotKeyString.Append("WIN + ");
            if ((ModifierKeys.Control & modifiers) == ModifierKeys.Control)
                hotKeyString.Append("CTRL + ");
            if ((ModifierKeys.Alt & modifiers) == ModifierKeys.Alt)
                hotKeyString.Append("ALT + ");
            if ((ModifierKeys.Shift & modifiers) == ModifierKeys.Shift)
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

        protected WindowInteropHelper window;

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
            if (NativeMethods.RegisterHotKey(handle, ++id, key.modifiers, key.key)) {
                key.id = id;
                Hotkeys.Add(id, key);
                observableHotkeys.Add(key);
            }
            else Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
        }

        public bool Unregister(Hotkey key) {
            var index = IndexOf(key);
            if (index > 0) {
                if (NativeMethods.UnregisterHotKey(handle, index)) {
                    Hotkeys.Remove(index);
                    observableHotkeys.Remove(key);
                    return true;
                }
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                return false;
            }
            return false;
        }

        public void Clear() {
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

        public bool Remove(Hotkey item) {
            return Unregister(item);
        }

        public IEnumerator<Hotkey> GetEnumerator() {
            return Hotkeys.Values.GetEnumerator();
        }

        public int IndexOf(Hotkey item) {
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

        public void Insert(int index, Hotkey item) {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveAt(int index) {
            Remove(Hotkeys[index]);
        }
    }

    [Flags]
    [ComVisible(true)]
    [Editor("System.Windows.Forms.Design.ShortcutKeysEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        typeof (UITypeEditor))]
    [TypeConverter("System.Windows.Forms.KeysConverter")]
    public enum Keys {
        Modifiers = -65536,
        None = 0,
        LButton = 1,
        RButton = 2,
        Cancel = 3,
        MButton = 4,
        XButton1 = 5,
        XButton2 = 6,
        Back = 8,
        Tab = 9,
        LineFeed = 10,
        Clear = 12,
        Enter = 13,
        Return = 13,
        ShiftKey = 16,
        ControlKey = 17,
        Menu = 18,
        Pause = 19,
        CapsLock = 20,
        Capital = 20,
        KanaMode = 21,
        HanguelMode = 21,
        HangulMode = 21,
        JunjaMode = 23,
        FinalMode = 24,
        KanjiMode = 25,
        HanjaMode = 25,
        Escape = 27,
        IMEConvert = 28,
        IMENonconvert = 29,
        IMEAceept = 30,
        IMEAccept = 30,
        IMEModeChange = 31,
        Space = 32,
        Prior = 33,
        PageUp = 33,
        Next = 34,
        PageDown = 34,
        End = 35,
        Home = 36,
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        Select = 41,
        Print = 42,
        Execute = 43,
        PrintScreen = 44,
        Snapshot = 44,
        Insert = 45,
        Delete = 46,
        Help = 47,
        D0 = 48,
        D1 = 49,
        D2 = 50,
        D3 = 51,
        D4 = 52,
        D5 = 53,
        D6 = 54,
        D7 = 55,
        D8 = 56,
        D9 = 57,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LWin = 91,
        RWin = 92,
        Apps = 93,
        Sleep = 95,
        NumPad0 = 96,
        NumPad1 = 97,
        NumPad2 = 98,
        NumPad3 = 99,
        NumPad4 = 100,
        NumPad5 = 101,
        NumPad6 = 102,
        NumPad7 = 103,
        NumPad8 = 104,
        NumPad9 = 105,
        Multiply = 106,
        Add = 107,
        Separator = 108,
        Subtract = 109,
        Decimal = 110,
        Divide = 111,
        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123,
        F13 = 124,
        F14 = 125,
        F15 = 126,
        F16 = 127,
        F17 = 128,
        F18 = 129,
        F19 = 130,
        F20 = 131,
        F21 = 132,
        F22 = 133,
        F23 = 134,
        F24 = 135,
        NumLock = 144,
        Scroll = 145,
        LShiftKey = 160,
        RShiftKey = 161,
        LControlKey = 162,
        RControlKey = 163,
        LMenu = 164,
        RMenu = 165,
        BrowserBack = 166,
        BrowserForward = 167,
        BrowserRefresh = 168,
        BrowserStop = 169,
        BrowserSearch = 170,
        BrowserFavorites = 171,
        BrowserHome = 172,
        VolumeMute = 173,
        VolumeDown = 174,
        VolumeUp = 175,
        MediaNextTrack = 176,
        MediaPreviousTrack = 177,
        MediaStop = 178,
        MediaPlayPause = 179,
        LaunchMail = 180,
        SelectMedia = 181,
        LaunchApplication1 = 182,
        LaunchApplication2 = 183,
        Oem1 = 186,
        OemSemicolon = 186,
        Oemplus = 187,
        Oemcomma = 188,
        OemMinus = 189,
        OemPeriod = 190,
        OemQuestion = 191,
        Oem2 = 191,
        Oemtilde = 192,
        Oem3 = 192,
        Oem4 = 219,
        OemOpenBrackets = 219,
        OemPipe = 220,
        Oem5 = 220,
        Oem6 = 221,
        OemCloseBrackets = 221,
        Oem7 = 222,
        OemQuotes = 222,
        Oem8 = 223,
        Oem102 = 226,
        OemBackslash = 226,
        ProcessKey = 229,
        Packet = 231,
        Attn = 246,
        Crsel = 247,
        Exsel = 248,
        EraseEof = 249,
        Play = 250,
        Zoom = 251,
        NoName = 252,
        Pa1 = 253,
        OemClear = 254,
        KeyCode = 65535,
        Shift = 65536,
        Control = 131072,
        Alt = 262144,
    }

    public partial struct WM {
        internal const int GetHotkey = 0x33;
        internal const int Hotkey = 0x312;
        internal const int SetHotkey = 0x0032;
    }

    public partial class NativeMethods {
        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys uVirtKey);

        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}