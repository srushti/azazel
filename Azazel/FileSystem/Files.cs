using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Azazel.Extensions;

namespace Azazel.FileSystem {
    public class Launchables : List<Launchable> {
        public Launchables() {}

        public Launchables(IEnumerable<FileInfo> fileInfos) {
            foreach (var fileInfo in fileInfos) {
                var file = new File(fileInfo);
                if (!file.IsHidden) Add(file);
            }
        }

        public Launchables(params Launchable[] launchables) : this(((IEnumerable<Launchable>) launchables)) {}

        public Launchables(IEnumerable<Launchable> launchables) {
            foreach (var launchable in launchables) Add(launchable);
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public Launchable Get(int index) {
            if (index >= Count) return File.NULL;
            return base[index];
        }

        public Launchables Sort(string input, History history) {
            var context = new FileRankContext(input, history);
            var topList = new Launchables();
            var bottomList = new Launchables();
            foreach (var launchable in this) {
                if (history.Contains(launchable) || launchable.Name.EqualsIgnoreCase(input)) topList.Add(launchable);
                else bottomList.Add(launchable);
            }
            context.SortLaunchables(topList);
            context.SortLaunchables(bottomList);
            topList.AddRange(bottomList);
            return topList;
        }

        public void AddRange(Launchables launchables) {
            base.AddRange(launchables);
        }

        public Launchables Find(string searchString, History history) {
            if (string.IsNullOrEmpty(searchString)) return new Files();
            var result = new Launchables();
            var regex = Regex(searchString);
            ForEach(launchable => {
                        if (regex.IsMatch(launchable.Name))
                            result.Add(launchable);
                    });
            return result.Sort(searchString, history);
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