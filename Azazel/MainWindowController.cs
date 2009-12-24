using System.Windows;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.Logging;
using Azazel.PluggingIn;
using Azazel.Threading;
using Action=System.Action;

namespace Azazel {
    public class MainWindowController {
        private readonly AppFinder appFinder;
        private int index;
        private string input = "";
        private Launchables selectedFiles = new Launchables();
        private Token latestToken = new Token();
        private Thread thread = new Thread(() => { });
        private readonly LaunchableHandlers launchableHandlers;
        private readonly SelfPlugin selfPlugin;
        public event Action RefreshedResults = () => { };

        public MainWindowController(LaunchablePlugins launchablePlugins, CharacterPlugins characterPlugins, LaunchableHandlers launchableHandlers,
                                    SelfPlugin selfPlugin, PersistanceHelper persistanceHelper, AppSettings settings) {
            this.launchableHandlers = launchableHandlers;
            this.selfPlugin = selfPlugin;
            appFinder = new AppFinder(launchablePlugins, characterPlugins, selfPlugin.XStream, persistanceHelper);
            selfPlugin.RefreshRequested += (() => appFinder.LoadFiles(launchablePlugins));
            selfPlugin.AddAFolder += (() => appFinder.AddFolder(launchablePlugins));
            selfPlugin.ChangeShortcut += (() => new KeyboardShortcutChangeCommand(settings, persistanceHelper).Execute());
        }

        public void SetInput(string value, bool asynchronous) {
            if (asynchronous) {
                thread = new Thread(RefreshResults, value);
                thread.Start();
            }
            else RefreshResults(value);
        }

        private void RefreshResults(object parameter) {
            var value = (string) parameter;
            if (input == value) return;
            Token workingToken = latestToken = new Token();
            Launchables newFiles = value.Contains(input) && !selectedFiles.IsEmpty() ? appFinder.FindFiles(selectedFiles, value) : appFinder.FindFiles(value);
            if (!latestToken.Equals(workingToken)) {
                LogManager.WriteLog("token check failed with input {0} and value {1}", input, value);
                return;
            }
            input = value;
            selectedFiles = newFiles;
            index = 0;
            RefreshedResults();
        }

        public string CommandName {
            get { return SelectedCommand.Name; }
        }

        internal Launchable SelectedCommand {
            get { return selectedFiles.Get(index); }
        }

        public int ResultCount {
            get { return selectedFiles.Count; }
        }

        public void LaunchApp(string arguments) {
            try {
                SelectedCommand.Launch(arguments);
                selfPlugin.Recent.AddExecutedCommand(SelectedCommand, arguments);
            }
            catch (AzazelException e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            appFinder.AppLaunched(input, SelectedCommand);
        }

        public void LaunchAction(FileSystem.Action action) {
            action.Act();
            selfPlugin.Recent.AddExecutedCommand(SelectedCommand, action);
            appFinder.AppLaunched(input, SelectedCommand);
        }

        public Launchable Command(int relativeIndex) {
            int actualIndex = index + relativeIndex;
            if (selectedFiles.Count > actualIndex)
                return selectedFiles.Get(actualIndex);
            return File.Null;
        }

        public void MoveDown(int count) {
            if (index + count < selectedFiles.Count && index + count >= 0) index += count;
            else index = count < 0 ? 0 : selectedFiles.Count - 1;
        }

        public Actions GetActionsForSelectedCommand() {
            var actions = new Actions(SelectedCommand.Actions);
            actions.AddRange(launchableHandlers.ActionsFor(SelectedCommand));
            return actions;
        }

        public Launchable ImmediateResult(string inputText) {
            if (inputText.IsNullOrEmpty()) return File.Null;
            return appFinder.ExactMatch(inputText);
        }

        public void CancelSearch() {
            thread.Abort();
        }
    }

    internal class KeyboardShortcutChangeCommand {
        private readonly AppSettings settings;
        private readonly PersistanceHelper persistanceHelper;

        public KeyboardShortcutChangeCommand(AppSettings settings, PersistanceHelper persistanceHelper) {
            this.settings = settings;
            this.persistanceHelper = persistanceHelper;
        }

        public void Execute() {
            var window = new KeyboardShortcut(settings, persistanceHelper);
            window.Show();
            window.Activate();
        }
    }
}