using System.Windows;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Azazel {
    public class MainWindowController {
        private readonly AppFinder appFinder;
        private int index;
        private string input = "";
        private Launchables selectedFiles = new Launchables();

        public MainWindowController(LaunchablePlugins launchablePlugins, CharacterPlugins characterPlugins) {
            appFinder = new AppFinder(launchablePlugins, characterPlugins);
            SelfPlugin.INSTANCE.RefreshRequested += (() => appFinder.LoadFiles(launchablePlugins));
            SelfPlugin.INSTANCE.AddAFolder += (() => appFinder.AddFolder(launchablePlugins));
            SelfPlugin.INSTANCE.ChangeShortcut += (() => new KeyboardShortcutChangeCommand().Execute());
        }

        public string Input {
            set {
                if (input == value) return;
                if (value.Contains(input) && !selectedFiles.IsEmpty()) selectedFiles = appFinder.FindFiles(selectedFiles, value);
                else selectedFiles = appFinder.FindFiles(value);
                input = value;
                index = 0;
            }
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
            var window = new SelectKeyboardShortcut();
            window.Show();
            window.Activate();
        }
    }
}