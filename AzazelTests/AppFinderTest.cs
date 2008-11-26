using System;
using Azazel.FileSystem;
using Azazel.PluggingIn;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using NUnit.Framework;

namespace Azazel {
    [TestFixture]
    public class AppFinderTest {
        private AppFinder finder;

        [SetUp]
        public void SetUp() {
            finder = new AppFinder(new LaunchablePlugins(), new CharacterPlugins());
        }

        [Test]
        public void FindsApp() {
            Assert.AreEqual(Paths.Combine(Constants.QuickLaunch, "Shows Desktop.lnk"), ((File) finder.FindFiles("Show Desktop")[0]).FullName);
        }

        [Test]
        public void DoesntCrashOnSpecialCharacter() {
            Assert.AreNotEqual(0, finder.FindFiles("+").Count);
        }

        [Test]
        public void MethodName() {
            Analyzer analyzer = new StandardAnalyzer();

            // Store the index in memory:
            Directory directory = new RAMDirectory();
            // To store an index on disk, use this instead (note that the 
            // parameter true will overwrite the index in that directory
            // if one exists):
            //Directory directory = FSDirectory.getDirectory("/tmp/testindex", true);
            var iwriter = new IndexWriter(directory, analyzer, true);
            iwriter.SetMaxFieldLength(25000);
            var doc = new Document();
            String text = "This is the text to be indexed.";
            doc.Add(new Field("fieldname", text, Field.Store.YES, Field.Index.TOKENIZED));
            iwriter.AddDocument(doc);
            iwriter.Close();

            // Now search the index:
            var isearcher = new IndexSearcher(directory);
            // Parse a simple query that searches for "text":
            Query query = new QueryParser("fieldname", analyzer).Parse("text");
            Hits hits = isearcher.Search(query);
            Assert.AreEqual(1, hits.Length());
            // Iterate through the results:
            for (int i = 0; i < hits.Length(); i++) {
                Document hitDoc = hits.Doc(i);
                Assert.AreEqual("This is the text to be indexed.", hitDoc.Get("fieldname"));
            }
            isearcher.Close();
            directory.Close();
        }
    }
}