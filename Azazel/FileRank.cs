using System;
using System.Collections.Generic;
using Azazel.Extensions;
using Azazel.FileSystem;
using Azazel.Logging;

namespace Azazel {
    public class FileRank : IComparable<FileRank> {
        private readonly int rank;

        public FileRank(Launchable launchable, History history, string input) {
            if (history.IsExactMatch(input, launchable)) rank = RankValue.PerfectMatch;
            if (launchable.Name.EqualsIgnoreCase(input)) rank += RankValue.ExactMatch;
            rank -= launchable.Name.IndexOf(input[0].ToString(), StringComparison.InvariantCultureIgnoreCase);
            if (launchable.Name.ContainsIgnoreCase(input)) rank += RankValue.NameContains;
            if (history.Contains(launchable)) rank += RankValue.HistoryContainsFile;
            if (history.ContainsAsSubstring(input, launchable)) rank += RankValue.HistoryContainsAsSubstring;
            if (launchable is File) ++rank;
        }

        public static int operator -(FileRank x, FileRank y) {
            return x.rank - y.rank;
        }

        public int CompareTo(FileRank other) {
            return rank - other.rank;
        }

        public override string ToString() {
            return rank.ToString();
        }

        private struct RankValue {
            internal const int NameContains = 04;
            internal const int HistoryContainsFile = NameContains*32;
            internal const int HistoryContainsAsSubstring = HistoryContainsFile*32;
            internal const int ExactMatch = HistoryContainsAsSubstring*32;
            internal const int PerfectMatch = ExactMatch*32;
        }
    }

    internal class FileRankContext {
        private readonly string input;
        private readonly History history;
        private readonly Dictionary<Launchable, FileRank> fileRanks = new Dictionary<Launchable, FileRank>();

        public FileRankContext(string input, History history) {
            this.input = input;
            this.history = history;
        }

        private FileRank Rank(Launchable launchable) {
            FileRank fileRank;
            if (fileRanks.TryGetValue(launchable, out fileRank)) return fileRank;
            fileRank = new FileRank(launchable, history, input);
            fileRanks.Add(launchable, fileRank);
            return fileRank;
        }

        public void SortLaunchables(Launchables launchables) {
            try {
                launchables.Sort(LaunchableComparison);
            }
            catch (InvalidOperationException e) {
                LogManager.WriteLog(e);
            }
        }

        private int LaunchableComparison(Launchable x, Launchable y) {
            if (ReferenceEquals(x, y)) return 0;
            return Rank(y) - Rank(x);
        }
    }
}