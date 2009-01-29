using System;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Browser {
    public class Firefox : LaunchablePlugin {
        private static readonly string profilesPath = Paths.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                                    @"Mozilla\Firefox\Profiles");
        private static readonly ImageSource icon = UrlLauncher.BrowserIcon;

        public static Regex Regex {
            get {
                var expression = new StringBuilder();
                var nonCapturingAttributes1 = new[] {"ADD_DATE", "LAST_VISIT", "LAST_MODIFIED",};
                var nonCapturingAttributes2 = new[] {"ICON", "LAST_CHARSET", "ID",};
                expression.Append("<DT><A" + Attribute("HREF", true));
                foreach (string nonCapturingAttribute in nonCapturingAttributes1) expression.Append(Attribute(nonCapturingAttribute, false));
                expression.Append(Attribute("SHORTCUTURL", true));
                foreach (string nonCapturingAttribute in nonCapturingAttributes2)
                    expression.Append(Attribute(nonCapturingAttribute, false));
                expression.Append(">(.*)</A>");
                return new Regex(expression + ".*", RegexOptions.Multiline);
            }
        }

        public Launchables Launchables() {
            var bookmarks = new Launchables();
            if (!IsAvailable) return bookmarks;
            foreach (Folder profileFolder in new Folder(profilesPath).GetFolders()) {
                var bookmarksFile = profileFolder.GetFile("bookmarks.html");
                if (bookmarksFile.Exists()) bookmarks.AddRange(LoadFF2Bookmarks(bookmarksFile));
                bookmarksFile = profileFolder.GetFile("places.sqlite");
                if (bookmarksFile.Exists()) bookmarks.AddRange(LoadFF3Bookmarks(bookmarksFile.Copy("places_copy.sqlite")));
            }
            return bookmarks;
        }

        private static Launchables LoadFF3Bookmarks(File file) {
            var connection = new SQLiteConnection(@"Data Source=" + file.FullName + @";Version=3;New=False;Compress=True;");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                "select b.title,  p.url,k.keyword from moz_places p join moz_bookmarks b on p.id = b.fk left outer join moz_keywords k on keyword_id = k.id where b.title is not null";
            var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            var bookmarks = new Launchables();
            while (reader.Read())
                bookmarks.AddRange(CreateBookmarks(reader["title"].ToString(), reader["url"].ToString(), reader["keyword"].ToString()));
            return bookmarks;
        }

        public bool IsAvailable {
            get { return Folder.Exists(profilesPath); }
        }

        private static Bookmarks LoadFF2Bookmarks(File file) {
            var bookmarks = new Bookmarks();
            var matches = Regex.Matches(file.Contents());
            foreach (Match match in matches) bookmarks.AddRange(LaunchableForBookmark(match));
            return bookmarks;
        }

        private static Bookmarks LaunchableForBookmark(Match match) {
            var url = match.Groups[1].Value;
            var keyword = match.Groups[2].Value;
            var bookmarkDescription = match.Groups[3].Value;
            return CreateBookmarks(bookmarkDescription, url, keyword);
        }

        private static Bookmarks CreateBookmarks(string bookmarkDescription, string url, string keyword) {
            var bookmarks = new Bookmarks();
            if (!string.IsNullOrEmpty(bookmarkDescription)) bookmarks.Add(new Bookmark(bookmarkDescription, url, icon));
            if (!string.IsNullOrEmpty(keyword)) bookmarks.Add(new Bookmark(keyword, url, icon));
            return bookmarks;
        }

        private static string Attribute(string attributeName, bool capturing) {
            return "(?: " + attributeName + (capturing ? "=\"([^\"]*)\"" : "=\"[^\"]*\"") + ")?";
        }

        private class Bookmarks : Files {}
    }
}