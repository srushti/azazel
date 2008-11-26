using NUnit.Framework;
using Rhino.Mocks;

namespace Azazel.FileSystem {
    [TestFixture]
    public class FilesTest {
        private Files files;
        private File abcdFile;
        private File abdcFile;
        private readonly MockRepository mocks = new MockRepository();
        private HistoryTest.HistoryStub history;

        [SetUp]
        public void Setup() {
            files = new Files();
            abcdFile = File(mocks, "abcd");
            files.Add(abcdFile);
            abdcFile = File(mocks, "abdc");
            files.Add(abdcFile);
            history = new HistoryTest.HistoryStub();
        }

        [Test]
        public void IfNotSubstringMatchLeftmostFirstLetter() {
            mocks.ReplayAll();
            SortsAndAssertsOrder("c", new[] {abcdFile, abdcFile}, files);
            SortsAndAssertsOrder("d", new[] {abdcFile, abcdFile}, files);
        }

        [Test]
        public void SubstringMatchesRankHigher() {
            mocks.ReplayAll();
            SortsAndAssertsOrder("abc", new[] {abcdFile, abdcFile,}, files);
            SortsAndAssertsOrder("abD", new[] {abdcFile, abcdFile,}, files);
        }

        [Test]
        public void ExactMatchesRankHigherThanSubstring() {
            var adbFile = File(mocks, "abd");
            files.Add(adbFile);
            mocks.ReplayAll();
            SortsAndAssertsOrder("abD", new[] {adbFile, abdcFile, abcdFile,}, files);
        }

        [Test]
        public void ExistenceInHistoryHasPriority() {
            var abracadabraFile = File(mocks, "abracadabra");
            files.Add(abracadabraFile);
            mocks.ReplayAll();
            history.Add("x", abracadabraFile);
            SortsAndAssertsOrder("abd", new[] {abracadabraFile, abdcFile, abcdFile}, files);
        }

        [Test]
        public void StartsWithMatchOfInputIsHigherThanJustExistence() {
            var abracadabraFile = File(mocks, "abracadabra");
            files.Add(abracadabraFile);
            mocks.ReplayAll();
            history.Add("x", abracadabraFile);
            history.Add("abc", abdcFile);
            history.Add("bcd", abcdFile);
            SortsAndAssertsOrder("ab", new[] {abdcFile, abracadabraFile, abcdFile}, files);
        }

        [Test]
        public void ExactMatchOfInputIsHighestOfThemAll() {
            var abracadabraFile = File(mocks, "abracadabra");
            files.Add(abracadabraFile);
            mocks.ReplayAll();
            history.Add("x", abracadabraFile);
            history.Add("xabcd", abdcFile);
            SortsAndAssertsOrder("x", new[] {abracadabraFile, abdcFile, abcdFile}, files);
        }

        private void SortsAndAssertsOrder(string searchString, File[] expectedFiles, Launchables actualFiles) {
            var sortedFiles = actualFiles.Sort(searchString, history);
            for (var i = 0; i < expectedFiles.Length; i++) Assert.AreSame(expectedFiles[i], sortedFiles.Get(i), "at index: " + i);
        }

        private static File File(MockRepository mocks, string fileName) {
            var file = mocks.PartialMock<File>();
            SetupResult.For(file.Name).Return(fileName);
            SetupResult.For(file.FullName).Return(fileName);
            SetupResult.For(file.GetHashCode()).Return(1);
            return file;
        }
    }
}