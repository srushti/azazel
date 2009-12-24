using NUnit.Framework;

namespace Azazel.FileSystem {
    [TestFixture]
    public class FoldersTest {
        [Test]
        public void CannotAddTheSameFolder() {
            var folders = new Folders();
            folders.Add(new Folder("."));
            folders.Add(new Folder("."));
            folders.ShouldHaveCount(1);
        }
    }
}