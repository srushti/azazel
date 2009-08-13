using System;
using Azazel.FileSystem;
using NUnit.Framework;
using Rhino.Mocks;

namespace Azazel {
    [TestFixture]
    public class HistoryTest {
        private History history;
        private static readonly Random random = new Random();
        private MockRepository mocks;

        [SetUp]
        public void Setup() {
            mocks = new MockRepository();
            history = new HistoryStub();
        }

        [Test]
        public void ForgetsPreviousCommandIfDifferentOneUsed() {
            var file1 = File(mocks, 1);
            var file2 = File(mocks, 2);
            mocks.ReplayAll();
            history.Add("1", file1);
            Assert.AreSame(file1, history["1"]);
            history.Add("1", file2);
            Assert.AreSame(file2, history["1"]);
        }

        [Test]
        public void RemembersACommand() {
            var file = File(mocks, 1);
            mocks.ReplayAll();
            history.Add("sh", file);
            Assert.AreEqual(file, history["sh"]);
        }

        public static File File(MockRepository mocks, int id) {
            var file = mocks.PartialMock<File>();
            SetupResult.For(file.GetHashCode()).Return(random.Next());
            SetupResult.For(file.FullName).Return(id.ToString());
            SetupResult.For(file.Name).Return(id.ToString());
            return file;
        }

        public class HistoryStub : History {
            protected override void StoreHistory() {}
        }
    }
}