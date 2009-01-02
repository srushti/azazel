using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Venus.Browser {
    [TestFixture]
    public class BookmarkTest {
        [Test]
        public void ReplacesCommandLineArguments() {
            var bookmark = new Bookmark("", "www.somesillysearchsite.com?q=%s", null);
            Assert.That(UrlLauncher.CommandToExecute(bookmark.FullName, "something").Arguments, Text.Contains("www.somesillysearchsite.com?q=something"));
        }

        [Test]
        public void EscapesCommandLineArgumentsBeforeReplacing() {
            var bookmark = new Bookmark("", "www.somesillysearchsite.com?q=%s", null);
            Assert.That(UrlLauncher.CommandToExecute(bookmark.FullName, "something else").Arguments,
                        Text.Contains("www.somesillysearchsite.com?q=something%20else"));
        }
    }
}