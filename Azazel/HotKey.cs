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

// WPF based

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

        /// <summary>
        /// Parses a hotkey from a string representation, like:
        /// <list type="">
        /// <item>win|ctrl|A</item>
        /// <item>win+shift+B</item>
        /// <item>ctrl+alt|OemTilde</item>
        /// </list>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
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

    /// <summary>
    /// The Hotkey manager
    /// </summary>
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

        /// <summary>Free all hotkey resources...
        /// </summary>
        public void Dispose() {
            Clear();
            //			ReleaseHandle();
            handle = IntPtr.Zero;
        }

        public event HotkeyPressedEvent HotkeyPressed;

        private void OnSourceInitialized(object sender, EventArgs e) {
            window = new WindowInteropHelper(actualWindow);
            handle = window.Handle;

            HwndSource.FromHwnd(handle).AddHook(WndProc);

            Hotkeys = new Dictionary<int, Hotkey>();
            observableHotkeys = new ObservableCollection<Hotkey>();
            // AssignHandle(handle);
        }

        /// <summary>
        /// Gets the hotkey from the Id sent by Windows in the WM.HOTKEY message
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns></returns>
        public Hotkey GetKey(int Id) {
            return Hotkeys[Id];
        }

        /// <summary>Register a new hotkey, and add it to our collection
        /// </summary>
        /// <param name="key">A reference to the Hotkey.</param>
        /// <returns>True if the hotkey was set successfully.</returns>
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

        /// <summary>Unregister the specified hotkey if it's in our collection
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if we successfully unregister the hotkey.</returns>
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

        /// <summary>Clears the hotkey collection
        /// </summary>
        public void Clear() {
            foreach (var key in Hotkeys)
                NativeMethods.UnregisterHotKey(handle, key.Key);
            // clear afterward so we don't break our enumeration
            Hotkeys.Clear();
        }

        /// <summary>
        /// Override the base WndProc ... but in WPF it's the old-fashioned multi-parameter WndProc
        /// <remarks>
        /// The .Net Framework is starting to feel ridiculously cobbled together ... why on earth 
        /// should WPF apps need to register WndProc's any differently than Windows.Forms apps?
        /// </remarks>
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">The high word</param>
        /// <param name="lParam">The low word</param>
        /// <param name="handled"><c>true</c> if the message was already handled.</param>
        /// <returns>IntPtr.Zero - I have no idea what this is supposed to return.</returns>
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
        // Summary:
        //     The bitmask to extract modifiers from a key value.
        Modifiers = -65536,
        //
        // Summary:
        //     No key pressed.
        None = 0,
        //
        // Summary:
        //     The left mouse button.
        LButton = 1,
        //
        // Summary:
        //     The right mouse button.
        RButton = 2,
        //
        // Summary:
        //     The CANCEL key.
        Cancel = 3,
        //
        // Summary:
        //     The middle mouse button (three-button mouse).
        MButton = 4,
        //
        // Summary:
        //     The first x mouse button (five-button mouse).
        XButton1 = 5,
        //
        // Summary:
        //     The second x mouse button (five-button mouse).
        XButton2 = 6,
        //
        // Summary:
        //     The BACKSPACE key.
        Back = 8,
        //
        // Summary:
        //     The TAB key.
        Tab = 9,
        //
        // Summary:
        //     The LINEFEED key.
        LineFeed = 10,
        //
        // Summary:
        //     The CLEAR key.
        Clear = 12,
        //
        // Summary:
        //     The ENTER key.
        Enter = 13,
        //
        // Summary:
        //     The RETURN key.
        Return = 13,
        //
        // Summary:
        //     The SHIFT key.
        ShiftKey = 16,
        //
        // Summary:
        //     The CTRL key.
        ControlKey = 17,
        //
        // Summary:
        //     The ALT key.
        Menu = 18,
        //
        // Summary:
        //     The PAUSE key.
        Pause = 19,
        //
        // Summary:
        //     The CAPS LOCK key.
        CapsLock = 20,
        //
        // Summary:
        //     The CAPS LOCK key.
        Capital = 20,
        //
        // Summary:
        //     The IME Kana mode key.
        KanaMode = 21,
        //
        // Summary:
        //     The IME Hanguel mode key. (maintained for compatibility; use HangulMode)
        HanguelMode = 21,
        //
        // Summary:
        //     The IME Hangul mode key.
        HangulMode = 21,
        //
        // Summary:
        //     The IME Junja mode key.
        JunjaMode = 23,
        //
        // Summary:
        //     The IME final mode key.
        FinalMode = 24,
        //
        // Summary:
        //     The IME Kanji mode key.
        KanjiMode = 25,
        //
        // Summary:
        //     The IME Hanja mode key.
        HanjaMode = 25,
        //
        // Summary:
        //     The ESC key.
        Escape = 27,
        //
        // Summary:
        //     The IME convert key.
        IMEConvert = 28,
        //
        // Summary:
        //     The IME nonconvert key.
        IMENonconvert = 29,
        //
        // Summary:
        //     The IME accept key. Obsolete, use System.Windows.Forms.Keys.IMEAccept instead.
        IMEAceept = 30,
        //
        // Summary:
        //     The IME accept key, replaces System.Windows.Forms.Keys.IMEAceept.
        IMEAccept = 30,
        //
        // Summary:
        //     The IME mode change key.
        IMEModeChange = 31,
        //
        // Summary:
        //     The SPACEBAR key.
        Space = 32,
        //
        // Summary:
        //     The PAGE UP key.
        Prior = 33,
        //
        // Summary:
        //     The PAGE UP key.
        PageUp = 33,
        //
        // Summary:
        //     The PAGE DOWN key.
        Next = 34,
        //
        // Summary:
        //     The PAGE DOWN key.
        PageDown = 34,
        //
        // Summary:
        //     The END key.
        End = 35,
        //
        // Summary:
        //     The HOME key.
        Home = 36,
        //
        // Summary:
        //     The LEFT ARROW key.
        Left = 37,
        //
        // Summary:
        //     The UP ARROW key.
        Up = 38,
        //
        // Summary:
        //     The RIGHT ARROW key.
        Right = 39,
        //
        // Summary:
        //     The DOWN ARROW key.
        Down = 40,
        //
        // Summary:
        //     The SELECT key.
        Select = 41,
        //
        // Summary:
        //     The PRINT key.
        Print = 42,
        //
        // Summary:
        //     The EXECUTE key.
        Execute = 43,
        //
        // Summary:
        //     The PRINT SCREEN key.
        PrintScreen = 44,
        //
        // Summary:
        //     The PRINT SCREEN key.
        Snapshot = 44,
        //
        // Summary:
        //     The INS key.
        Insert = 45,
        //
        // Summary:
        //     The DEL key.
        Delete = 46,
        //
        // Summary:
        //     The HELP key.
        Help = 47,
        //
        // Summary:
        //     The 0 key.
        D0 = 48,
        //
        // Summary:
        //     The 1 key.
        D1 = 49,
        //
        // Summary:
        //     The 2 key.
        D2 = 50,
        //
        // Summary:
        //     The 3 key.
        D3 = 51,
        //
        // Summary:
        //     The 4 key.
        D4 = 52,
        //
        // Summary:
        //     The 5 key.
        D5 = 53,
        //
        // Summary:
        //     The 6 key.
        D6 = 54,
        //
        // Summary:
        //     The 7 key.
        D7 = 55,
        //
        // Summary:
        //     The 8 key.
        D8 = 56,
        //
        // Summary:
        //     The 9 key.
        D9 = 57,
        //
        // Summary:
        //     The A key.
        A = 65,
        //
        // Summary:
        //     The B key.
        B = 66,
        //
        // Summary:
        //     The C key.
        C = 67,
        //
        // Summary:
        //     The D key.
        D = 68,
        //
        // Summary:
        //     The E key.
        E = 69,
        //
        // Summary:
        //     The F key.
        F = 70,
        //
        // Summary:
        //     The G key.
        G = 71,
        //
        // Summary:
        //     The H key.
        H = 72,
        //
        // Summary:
        //     The I key.
        I = 73,
        //
        // Summary:
        //     The J key.
        J = 74,
        //
        // Summary:
        //     The K key.
        K = 75,
        //
        // Summary:
        //     The L key.
        L = 76,
        //
        // Summary:
        //     The M key.
        M = 77,
        //
        // Summary:
        //     The N key.
        N = 78,
        //
        // Summary:
        //     The O key.
        O = 79,
        //
        // Summary:
        //     The P key.
        P = 80,
        //
        // Summary:
        //     The Q key.
        Q = 81,
        //
        // Summary:
        //     The R key.
        R = 82,
        //
        // Summary:
        //     The S key.
        S = 83,
        //
        // Summary:
        //     The U key.
        T = 84,
        //
        // Summary:
        //     The V key.
        U = 85,
        //
        // Summary:
        //     The V key.
        V = 86,
        //
        // Summary:
        //     The W key.
        W = 87,
        //
        // Summary:
        //     The X key.
        X = 88,
        //
        // Summary:
        //     The Y key.
        Y = 89,
        //
        // Summary:
        //     The Z key.
        Z = 90,
        //
        // Summary:
        //     The left Windows logo key (Microsoft Natural Keyboard).
        LWin = 91,
        //
        // Summary:
        //     The right Windows logo key (Microsoft Natural Keyboard).
        RWin = 92,
        //
        // Summary:
        //     The application key (Microsoft Natural Keyboard).
        Apps = 93,
        //
        // Summary:
        //     The computer sleep key.
        Sleep = 95,
        //
        // Summary:
        //     The 0 key on the numeric keypad.
        NumPad0 = 96,
        //
        // Summary:
        //     The 1 key on the numeric keypad.
        NumPad1 = 97,
        //
        // Summary:
        //     The 2 key on the numeric keypad.
        NumPad2 = 98,
        //
        // Summary:
        //     The 3 key on the numeric keypad.
        NumPad3 = 99,
        //
        // Summary:
        //     The 4 key on the numeric keypad.
        NumPad4 = 100,
        //
        // Summary:
        //     The 5 key on the numeric keypad.
        NumPad5 = 101,
        //
        // Summary:
        //     The 6 key on the numeric keypad.
        NumPad6 = 102,
        //
        // Summary:
        //     The 7 key on the numeric keypad.
        NumPad7 = 103,
        //
        // Summary:
        //     The 8 key on the numeric keypad.
        NumPad8 = 104,
        //
        // Summary:
        //     The 9 key on the numeric keypad.
        NumPad9 = 105,
        //
        // Summary:
        //     The multiply key.
        Multiply = 106,
        //
        // Summary:
        //     The add key.
        Add = 107,
        //
        // Summary:
        //     The separator key.
        Separator = 108,
        //
        // Summary:
        //     The subtract key.
        Subtract = 109,
        //
        // Summary:
        //     The decimal key.
        Decimal = 110,
        //
        // Summary:
        //     The divide key.
        Divide = 111,
        //
        // Summary:
        //     The F1 key.
        F1 = 112,
        //
        // Summary:
        //     The F2 key.
        F2 = 113,
        //
        // Summary:
        //     The F3 key.
        F3 = 114,
        //
        // Summary:
        //     The F4 key.
        F4 = 115,
        //
        // Summary:
        //     The F5 key.
        F5 = 116,
        //
        // Summary:
        //     The F6 key.
        F6 = 117,
        //
        // Summary:
        //     The F7 key.
        F7 = 118,
        //
        // Summary:
        //     The F8 key.
        F8 = 119,
        //
        // Summary:
        //     The F9 key.
        F9 = 120,
        //
        // Summary:
        //     The F10 key.
        F10 = 121,
        //
        // Summary:
        //     The F11 key.
        F11 = 122,
        //
        // Summary:
        //     The F12 key.
        F12 = 123,
        //
        // Summary:
        //     The F13 key.
        F13 = 124,
        //
        // Summary:
        //     The F14 key.
        F14 = 125,
        //
        // Summary:
        //     The F15 key.
        F15 = 126,
        //
        // Summary:
        //     The F16 key.
        F16 = 127,
        //
        // Summary:
        //     The F17 key.
        F17 = 128,
        //
        // Summary:
        //     The F18 key.
        F18 = 129,
        //
        // Summary:
        //     The F19 key.
        F19 = 130,
        //
        // Summary:
        //     The F20 key.
        F20 = 131,
        //
        // Summary:
        //     The F21 key.
        F21 = 132,
        //
        // Summary:
        //     The F22 key.
        F22 = 133,
        //
        // Summary:
        //     The F23 key.
        F23 = 134,
        //
        // Summary:
        //     The F24 key.
        F24 = 135,
        //
        // Summary:
        //     The NUM LOCK key.
        NumLock = 144,
        //
        // Summary:
        //     The SCROLL LOCK key.
        Scroll = 145,
        //
        // Summary:
        //     The left SHIFT key.
        LShiftKey = 160,
        //
        // Summary:
        //     The right SHIFT key.
        RShiftKey = 161,
        //
        // Summary:
        //     The left CTRL key.
        LControlKey = 162,
        //
        // Summary:
        //     The right CTRL key.
        RControlKey = 163,
        //
        // Summary:
        //     The left ALT key.
        LMenu = 164,
        //
        // Summary:
        //     The right ALT key.
        RMenu = 165,
        //
        // Summary:
        //     The browser back key (Windows 2000 or later).
        BrowserBack = 166,
        //
        // Summary:
        //     The browser forward key (Windows 2000 or later).
        BrowserForward = 167,
        //
        // Summary:
        //     The browser refresh key (Windows 2000 or later).
        BrowserRefresh = 168,
        //
        // Summary:
        //     The browser stop key (Windows 2000 or later).
        BrowserStop = 169,
        //
        // Summary:
        //     The browser search key (Windows 2000 or later).
        BrowserSearch = 170,
        //
        // Summary:
        //     The browser favorites key (Windows 2000 or later).
        BrowserFavorites = 171,
        //
        // Summary:
        //     The browser home key (Windows 2000 or later).
        BrowserHome = 172,
        //
        // Summary:
        //     The volume mute key (Windows 2000 or later).
        VolumeMute = 173,
        //
        // Summary:
        //     The volume down key (Windows 2000 or later).
        VolumeDown = 174,
        //
        // Summary:
        //     The volume up key (Windows 2000 or later).
        VolumeUp = 175,
        //
        // Summary:
        //     The media next track key (Windows 2000 or later).
        MediaNextTrack = 176,
        //
        // Summary:
        //     The media previous track key (Windows 2000 or later).
        MediaPreviousTrack = 177,
        //
        // Summary:
        //     The media Stop key (Windows 2000 or later).
        MediaStop = 178,
        //
        // Summary:
        //     The media play pause key (Windows 2000 or later).
        MediaPlayPause = 179,
        //
        // Summary:
        //     The launch mail key (Windows 2000 or later).
        LaunchMail = 180,
        //
        // Summary:
        //     The select media key (Windows 2000 or later).
        SelectMedia = 181,
        //
        // Summary:
        //     The start application one key (Windows 2000 or later).
        LaunchApplication1 = 182,
        //
        // Summary:
        //     The start application two key (Windows 2000 or later).
        LaunchApplication2 = 183,
        //
        // Summary:
        //     The OEM 1 key.
        Oem1 = 186,
        //
        // Summary:
        //     The OEM Semicolon key on a US standard keyboard (Windows 2000 or later).
        OemSemicolon = 186,
        //
        // Summary:
        //     The OEM plus key on any country/region keyboard (Windows 2000 or later).
        Oemplus = 187,
        //
        // Summary:
        //     The OEM comma key on any country/region keyboard (Windows 2000 or later).
        Oemcomma = 188,
        //
        // Summary:
        //     The OEM minus key on any country/region keyboard (Windows 2000 or later).
        OemMinus = 189,
        //
        // Summary:
        //     The OEM period key on any country/region keyboard (Windows 2000 or later).
        OemPeriod = 190,
        //
        // Summary:
        //     The OEM question mark key on a US standard keyboard (Windows 2000 or later).
        OemQuestion = 191,
        //
        // Summary:
        //     The OEM 2 key.
        Oem2 = 191,
        //
        // Summary:
        //     The OEM tilde key on a US standard keyboard (Windows 2000 or later).
        Oemtilde = 192,
        //
        // Summary:
        //     The OEM 3 key.
        Oem3 = 192,
        //
        // Summary:
        //     The OEM 4 key.
        Oem4 = 219,
        //
        // Summary:
        //     The OEM open bracket key on a US standard keyboard (Windows 2000 or later).
        OemOpenBrackets = 219,
        //
        // Summary:
        //     The OEM pipe key on a US standard keyboard (Windows 2000 or later).
        OemPipe = 220,
        //
        // Summary:
        //     The OEM 5 key.
        Oem5 = 220,
        //
        // Summary:
        //     The OEM 6 key.
        Oem6 = 221,
        //
        // Summary:
        //     The OEM close bracket key on a US standard keyboard (Windows 2000 or later).
        OemCloseBrackets = 221,
        //
        // Summary:
        //     The OEM 7 key.
        Oem7 = 222,
        //
        // Summary:
        //     The OEM singled/double quote key on a US standard keyboard (Windows 2000
        //     or later).
        OemQuotes = 222,
        //
        // Summary:
        //     The OEM 8 key.
        Oem8 = 223,
        //
        // Summary:
        //     The OEM 102 key.
        Oem102 = 226,
        //
        // Summary:
        //     The OEM angle bracket or backslash key on the RT 102 key keyboard (Windows
        //     2000 or later).
        OemBackslash = 226,
        //
        // Summary:
        //     The PROCESS KEY key.
        ProcessKey = 229,
        //
        // Summary:
        //     Used to pass Unicode characters as if they were keystrokes. The Packet key
        //     value is the low word of a 32-bit virtual-key value used for non-keyboard
        //     input methods.
        Packet = 231,
        //
        // Summary:
        //     The ATTN key.
        Attn = 246,
        //
        // Summary:
        //     The CRSEL key.
        Crsel = 247,
        //
        // Summary:
        //     The EXSEL key.
        Exsel = 248,
        //
        // Summary:
        //     The ERASE EOF key.
        EraseEof = 249,
        //
        // Summary:
        //     The PLAY key.
        Play = 250,
        //
        // Summary:
        //     The ZOOM key.
        Zoom = 251,
        //
        // Summary:
        //     A constant reserved for future use.
        NoName = 252,
        //
        // Summary:
        //     The PA1 key.
        Pa1 = 253,
        //
        // Summary:
        //     The CLEAR key.
        OemClear = 254,
        //
        // Summary:
        //     The bitmask to extract a key code from a key value.
        KeyCode = 65535,
        //
        // Summary:
        //     The SHIFT modifier key.
        Shift = 65536,
        //
        // Summary:
        //     The CTRL modifier key.
        Control = 131072,
        //
        // Summary:
        //     The ALT modifier key.
        Alt = 262144,
    }

    public partial struct WM {
        internal const int GetHotkey = 0x33;
        internal const int Hotkey = 0x312;
        internal const int SetHotkey = 0x0032;
    }

    public partial class NativeMethods {
        /// <summary>
        /// The RegisterHotKey function defines a system-wide hot key
        /// </summary>
        /// <param name="hWnd">Handle to the window that will receive WM_HOTKEY messages generated by the hot key. If this parameter is NULL, WM_HOTKEY messages are posted to the message queue of the calling thread and must be processed in the message loop.</param>
        /// <param name="id">Specifies the identifier of the hot key. No other hot key in the calling thread should have the same identifier. An application must specify a value in the range 0x0000 through 0xBFFF. A shared dynamic-link library (DLL) must specify a value in the range 0xC000 through 0xFFFF (the range returned by the GlobalAddAtom function). To avoid conflicts with hot-key identifiers defined by other shared DLLs, a DLL should use the GlobalAddAtom function to obtain the hot-key identifier.</param>
        /// <param name="fsModifiers">Specifies keys that must be pressed in combination with the key specified by the uVirtKey parameter in order to generate the WM_HOTKEY message.</param>
        /// <param name="uVirtKey">Specifies the virtual-key code of the hot key.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys uVirtKey);

        /// <summary>
        /// The UnregisterHotKey function frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">Handle to the window associated with the hot key to be freed. This parameter should be NULL if the hot key is not associated with a window.</param>
        /// <param name="id">Specifies the identifier of the hot key to be freed.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("User32", SetLastError = true)]
        [return : MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}