using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azazel;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Calculate {
    public class CalculatePlugin : CharacterPlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public bool IsValidFor(string searchString) {
            return searchString.Length >= 3 && ContainsMathematicalExpressions(searchString);
        }

        private static bool ContainsMathematicalExpressions(string s) {
            return Regex.IsMatch(s, @"^[^a-zA-Z]+$") && new CalculateLaunchable(s).Compiled;
        }

        public Launchable Launchable(string searchString) {
            return new CalculateLaunchable(searchString);
        }

        public class CalculateLaunchable : Launchable {
            private readonly string result;
            private static string lastUsedEquation;
            private static string lastCalculatedResult;
            private static readonly Regex numberOperatorRegex = new Regex(@"([0-9\.]+)([\(\)\+\-\*\/ ]+)", RegexOptions.Compiled);
            private static readonly Regex startingOperatorRegex = new Regex(@"^([\(\)\+\-\*\/ ]+)", RegexOptions.Compiled);

            public CalculateLaunchable(string equation) {
                if (Equals(lastUsedEquation, equation)) {
                    result = lastCalculatedResult;
                    return;
                }
                string floated = FloatedEquation(equation);
                lastUsedEquation = equation;
//                using (var creator = new TemporaryDomainTypeCreator<CodeDomCalculator>())
//                    lastCalculatedResult = result = creator.Create(floated).DoIt();
                lastCalculatedResult = result = new CodeDomCalculator(floated).DoIt();
            }

            private static string FloatedEquation(string equation) {
                var matches = numberOperatorRegex.Matches(equation.Replace(" ", "") + " ");
                var startingMatch = startingOperatorRegex.Match(equation);
                var builder = new StringBuilder(startingMatch.Value);
                foreach (Match match in matches)
                    builder.Append(match.Result("((double)$1)$2"));
                return builder.ToString();
            }

            public string Name {
                get { return result; }
            }

            public ImageSource Icon {
                get { return new BitmapImage(); }
            }

            public Actions Actions {
                get { return new Actions(); }
            }

            public void Launch(string arguments) {
                Clipboard.AddString(result);
            }

            public bool ShouldStoreHistory {
                get { return false; }
            }

            public bool Compiled {
                get { return !string.IsNullOrEmpty(result); }
            }
        }
    }
}
