using NUnit.Framework;
using Venus.Browser;

namespace Venus.Browser {
    [TestFixture]
    public class BookmarkTest {
        [Test]
        public void ReplacesCommandLineArguments() {
            var bookmark = new Bookmark("", "www.somesillysearchsite.com?q=%s", null);
            Assert.AreEqual("www.somesillysearchsite.com?q=something", UrlLauncher.CommandToExecute(bookmark.FullName, "something"));
        }

        [Test]
        public void EscapesCommandLineArgumentsBeforeReplacing() {
            var bookmark = new Bookmark("", "www.somesillysearchsite.com?q=%s", null);
            Assert.AreEqual("www.somesillysearchsite.com?q=something%20else", UrlLauncher.CommandToExecute(bookmark.FullName, "something else"));
        }
    }
}