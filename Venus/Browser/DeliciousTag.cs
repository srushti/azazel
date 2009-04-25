using System.Reflection;
using System.Windows.Media;
using Azazel.FileSystem;

namespace Venus.Browser {
    internal class DeliciousTag : Launchable {
        private readonly string tag;
        private readonly int id;
        private readonly File sqliteFile;

        public DeliciousTag(string tag, int id, File sqliteFile) {
            this.tag = tag;
            this.id = id;
            this.sqliteFile = sqliteFile;
        }

        public string Name {
            get { return tag; }
        }

        public ImageSource Icon {
            get {
                return
                    IconExtractor.Instance.Extract(
                        new File(Paths.Combine(new File(Assembly.GetExecutingAssembly().FullName).ParentFolder.FullName, "delicious.ico")));
            }
        }

        public Actions Actions {
            get {
                using (var connection = new Connection(sqliteFile)) {
                    var reader =
                        connection.ExecuteQuery(
                            "SELECT b.name as title, b.url FROM bookmarks b inner join bookmarks_tags bt on bt.bookmark_id = b.rowid where title is not null and bt.tag_id = " +
                            id);
                    var actions = new Actions();
                    while (reader.Read()) actions.Add(new DeliciousAction(reader["title"].ToString(), reader["url"].ToString()));
                    return actions;
                }
            }
        }

        public void Launch(string arguments) {}

        public bool ShouldStoreHistory {
            get { return false; }
        }
    }

    internal class DeliciousAction : Action {
        private readonly string title;
        private readonly string url;

        public DeliciousAction(string title, string url) {
            this.title = title;
            this.url = url;
        }

        public void Act() {
            UrlLauncher.CommandToExecute(url, "").AsyncStart();
        }

        public string Name {
            get { return title; }
        }
    }
}