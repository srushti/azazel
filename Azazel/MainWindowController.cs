using System.Windows;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.Logging;
using Azazel.PluggingIn;
using Azazel.Threading;

namespace Azazel {
    public class MainWindowController {
        private readonly AppFinder appFinder;
        private int index;
        private string input = "";
        private Launchables selectedFiles = new Launchables();
        private Token latestToken = new Token();
        private Thread thread = new Thread(() => { });
        public event VoidDelegate RefreshedResults = () => { };

        public MainWindowController(LaunchablePlugins launchablePlugins, CharacterPlugins characterPlugins) {
            appFinder = new AppFinder(launchablePlugins, characterPlugins);
            SelfPlugin.INSTANCE.RefreshRequested += (() => appFinder.LoadFiles(launchablePlugins));
            SelfPlugin.INSTANCE.AddAFolder += (() => appFinder.AddFolder(launchablePlugins));
            SelfPlugin.INSTANCE.ChangeShortcut += (() => new KeyboardShortcutChangeCommand().Execute());
        }

        public void SetInput(string value, bool asynchronous) {
            thread.Abort();
            if (asynchronous) {
                thread = new Thread(RefreshResults, value);
                thread.Start();
            }
            else RefreshResults(value);
        }

        private void RefreshResults(object parameter) {
            var value = (string) parameter;
            if (input == value) return;
            var workingToken = latestToken = new Token();
            var newFiles = value.Contains(input) && !selectedFiles.IsEmpty() ? appFinder.FindFiles(selectedFiles, value) : appFinder.FindFiles(value);
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

        public void LaunchApp(string arguments) {
            try {
                SelectedCommand.Launch(arguments);
                SelfPlugin.INSTANCE.Recent.AddExecutedCommand(SelectedCommand, arguments);
            }
            catch (AzazelException e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            appFinder.AppLaunched(input, SelectedCommand);
        }

        public void LaunchAction(Action action) {
            action.Act();
            SelfPlugin.INSTANCE.Recent.AddExecutedCommand(SelectedCommand, action);
            appFinder.AppLaunched(input, SelectedCommand);
        }

        public Launchable Command(int relativeIndex) {
            int actualIndex = index + relativeIndex;
            if (selectedFiles.Count > actualIndex)
                return selectedFiles.Get(actualIndex);
            return File.NULL;
        }

        public void MoveDown(int count) {
            if (index + count < selectedFiles.Count && index + count >= 0) index += count;
            else index = count < 0 ? 0 : selectedFiles.Count - 1;
        }
    }

    internal class KeyboardShortcutChangeCommand {
        public void Execute() {
            var window = new KeyboardShortcut();
            window.Show();
            window.Activate();
        }
    }
}