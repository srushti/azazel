using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Azazel.Extensions;

namespace Azazel.FileSystem {
    public class Launchables : List<Launchable> {
        public Launchables() {}

        protected Launchables(IEnumerable<FileInfo> fileInfos) {
            foreach (var fileInfo in fileInfos) {
                var file = new File(fileInfo);
                if (!file.IsHidden) Add(file);
            }
        }

        public Launchables(params Launchable[] launchables) : this(((IEnumerable<Launchable>) launchables)) {}

        private Launchables(IEnumerable<Launchable> launchables) {
            foreach (var launchable in launchables) Add(launchable);
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public Launchable Get(int index) {
            if (index >= Count) return File.NULL;
            return base[index];
        }

        public void AddRange(Launchables launchables) {
            base.AddRange(launchables);
        }

        public Launchables Find(string searchString, History history) {
            if (string.IsNullOrEmpty(searchString)) return new Files();
            var regex = Regex(searchString);
            var topList = new Launchables();
            var bottomList = new Launchables();
            ForEach(launchable => {
                        if (regex.IsMatch(launchable.Name)) {
                            if (history.Contains(launchable) || launchable.Name.EqualsIgnoreCase(searchString)) topList.Add(launchable);
                            else bottomList.Add(launchable);
                        }
                    });
            var context = new FileRankContext(searchString, history);
            context.SortLaunchables(topList);
            context.SortLaunchables(bottomList);
            topList.AddRange(bottomList);
            return topList;
        }

        private static Regex Regex(string searchString) {
            var pattern = new StringBuilder(".*");
            foreach (char c in searchString) pattern.Append(System.Text.RegularExpressions.Regex.Escape(c.ToString()) + ".*");
            return new Regex(pattern.ToString(), RegexOptions.IgnoreCase);
        }
    }

    public class Files : Launchables {
        public Files(IEnumerable<FileInfo> fileInfos) : base(fileInfos) {}

        public Files() {}
    }
}