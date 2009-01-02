using System.Windows.Media;

namespace Venus.Browser {
    public class Search : Bookmark {
        public readonly string SearchQuery;

        protected Search() {}

        public Search(string name, string url, string searchQuery, ImageSource icon) : base(name, url, icon) {
            SearchQuery = searchQuery;
        }

        public override void Launch(string arguments) {
            if (string.IsNullOrEmpty(arguments))
                Launch(FullName, arguments);
            Launch(SearchQuery, arguments);
        }
    }
}