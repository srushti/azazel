namespace Azazel {
    public partial class MainWindow {
        private class ArgumentEnteringState : CommandState {
            private readonly MainWindow window;

            public ArgumentEnteringState(MainWindow window) {
                this.window = window;
            }

            public void HandleEscape() {
                window.ResetCommand();
            }

            public void HandleTab() {}

            public void MoveDown(int count) {}
        }

        private class CommandSelectingState : CommandState {
            private readonly MainWindow window;

            public CommandSelectingState(MainWindow window) {
                this.window = window;
            }

            public void HandleEscape() {
                window.Collapse();
            }

            public void HandleTab() {
                window.SelectDetails();
            }

            public void MoveDown(int count) {
                window.MoveDown(count);
            }
        }

        private class OptionSelectingState : CommandState {
            private readonly MainWindow window;

            public OptionSelectingState(MainWindow window) {
                this.window = window;
            }

            public void HandleEscape() {
                window.ResetCommand();
            }

            public void HandleTab() {
                MoveDown(1);
            }

            public void MoveDown(int count) {}
        }
    }
}