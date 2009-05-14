using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.Logging;
using Azazel.Threading;
using Action=Azazel.FileSystem.Action;

namespace Azazel {
    public partial class MainWindow {
        private readonly MainWindowController controller;
        private readonly Storyboard displayOptionsStoryboard;
        private readonly Storyboard parameterStoryboard;
        private readonly Storyboard resetStoryboard;
        private readonly Storyboard showOtherLaunchablesStoryboard;
        private readonly Timer<string> timer;
        private bool closing;
        private CommandState state;

        public MainWindow() {
            state = new CommandSelectingState(this);
            InitializeComponent();
            HookEvents();
            timer = new Timer<string>(PresentInput);
            parameterStoryboard = (Storyboard) Resources["Parameter"];
            resetStoryboard = (Storyboard) Resources["Reset"];
            displayOptionsStoryboard = (Storyboard) Resources["DisplayOptions"];
            showOtherLaunchablesStoryboard = (Storyboard) Resources["ShowOtherLaunchables"];
        }

        public MainWindow(MainWindowController controller) : this() {
            this.controller = controller;
            controller.RefreshedResults += CallBack;
        }

        private void InitialiseControls() {
            input.Focus();
            input.Text = "";
        }

        private void HookEvents() {
            Activated += delegate { WindowActivated(); };
            KeyUp += (sender, e) => HandleKey(e.Key, Keyboard.Modifiers);
            Closing += WindowClosing;
            Deactivated += delegate { Collapse(); };
            input.TextChanged += delegate { InputChanged(); };
            Deactivated += delegate { ResetCommand(); };
            Loaded += delegate { InitialiseControls(); };
        }

        private void WindowActivated() {
            new WindowHider(this).Hide();
        }

        private void InputChanged() {
            var inputText = input.Text;
            if (inputText.IsNullOrEmpty()) {
                timer.Start(0, inputText);
                return;
            }
            var immediateResult = controller.ImmediateResult(inputText);
            if (immediateResult != null) {
                controller.CancelSearch();
                selectedCommand.Text = immediateResult.Name;
                image.Source = immediateResult.Icon;
                timer.Start(1, inputText);
            }
            else timer.Start(.5, inputText);
        }

        private void PresentInput(string inputText) {
            controller.SetInput(inputText, true);
        }

        private void CallBack() {
            if (!Dispatcher.Thread().Equals(Thread.CurrentThread))
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new VoidDelegate(RefreshMenu));
            else RefreshMenu();
        }

        private void RefreshMenu() {
            selectedCommand.Text = controller.CommandName;
            image.Source = Icon(controller.Command(0));
            command2Text.Text = controller.Command(1).Name;
            ((Image) command2.Bullet).Source = controller.Command(1).Icon;
            command3Text.Text = controller.Command(2).Name;
            ((Image) command3.Bullet).Source = controller.Command(2).Icon;
            if (controller.ResultCount > 1) BeginAnimation(showOtherLaunchablesStoryboard);
        }

        private new static ImageSource Icon(Launchable launchable) {
            try {
                return launchable.Icon;
            }
            catch (Exception exception) {
                LogManager.WriteLog(exception);
                return new BitmapImage();
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e) {
            if (!closing) e.Cancel = true;
        }

        private void HandleKey(Key key, ModifierKeys modifiers) {
            if (key == Key.Escape) state.HandleEscape();
            else if (key == Key.Up) state.MoveDown(-1);
            else if (key == Key.Down) state.MoveDown(1);
            else if (key == Key.PageUp) state.MoveDown(-3);
            else if (key == Key.PageDown) state.MoveDown(3);
            else if (key == Key.F4 && modifiers == ModifierKeys.Alt)
                Collapse();
            else if (key == Key.C && modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                UseSelectedFile(file => Clipboard.AddString(file.FullName));
            else if (key == Key.E && modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                UseSelectedFile(file => Runner.ExplorerRunner(file.ParentFolder).AsyncStart());
            else if (key == Key.Enter) {
                LoadFirstResult();
                LaunchApp();
            }
            else if (key == Key.Tab) {
                LoadFirstResult();
                state.HandleTab();
            }
            else return;
            RefreshMenu();
        }

        private void UseSelectedFile(UseFile useFile) {
            var file = controller.SelectedCommand as File;
            if (file != null)
                useFile(file);
        }

        private void LoadFirstResult() {
            timer.Stop();
            controller.SetInput(input.Text, false);
            RefreshMenu();
        }

        private void MoveDown(int count) {
            controller.MoveDown(count);
        }

        private void SelectDetails() {
            if (File.Null.Equals(controller.Command(0))) return;
            arguments.Focusable = true;
            var actions = controller.GetActionsForSelectedCommand();
            if (actions.IsEmpty()) AcceptParameters();
            else DisplayOptions(actions);
        }

        private void DisplayOptions(IEnumerable<Action> actions) {
            BeginAnimation(displayOptionsStoryboard);
            options.Items.Clear();
            foreach (var action in actions) options.Items.Add(new ActionItem(action));
            input.Focusable = false;
            options.Focusable = true;
            options.Focus();
            options.SelectedIndex = 0;
            state = new OptionSelectingState(this);
        }

        private void AcceptParameters() {
            input.Focusable = false;
            arguments.Clear();
            arguments.Focus();
            state = new ArgumentEnteringState(this);
            BeginAnimation(parameterStoryboard);
        }

        private void BeginAnimation(Storyboard storyboard) {
            if (storyboard == null) return;
            storyboard.Begin(this);
        }

        private void ResetCommand() {
            arguments.Focusable = false;
            input.Focusable = true;
            options.Focusable = false;
            options.Items.Clear();
            input.Focus();
            input.SelectAll();
            arguments.Text = string.Empty;
            state = new CommandSelectingState(this);
            BeginAnimation(resetStoryboard);
        }

        private void LaunchApp() {
            if (options.SelectedItem != null) controller.LaunchAction(((ActionItem) options.SelectedItem).Action);
            else controller.LaunchApp(arguments.Text.Trim());
            Collapse();
        }

        public void Collapse() {
            closing = true;
            ClearOut();
            try {
                Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException) {}
        }

        public void StartWorking() {
            Visibility = Visibility.Visible;
            input.Focus();
            ClearOut();
            Activate();
        }

        private void ClearOut() {
            input.Text = string.Empty;
            selectedCommand.Text = string.Empty;
            arguments.Text = string.Empty;
        }

        private interface CommandState {
            void HandleEscape();
            void HandleTab();
            void MoveDown(int count);
        }

        private class ActionItem {
            public readonly Action Action;

            public ActionItem(Action action) {
                Action = action;
            }

            public override string ToString() {
                return Action.Name;
            }
        }

        private delegate void UseFile(File file);
    }
}