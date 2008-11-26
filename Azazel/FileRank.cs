using System;
using System.Collections.Generic;
using Azazel.Extensions;
using Azazel.FileSystem;

namespace Azazel {
    public class FileRank {
        private readonly int rank;

        public FileRank(Launchable launchable, History history, string input) {
            if (history.IsExactMatch(input, launchable)) rank = RankValue.PerfectMatch;
            if (launchable.Name.Equals(input, StringComparison.InvariantCultureIgnoreCase)) rank += RankValue.ExactMatch;
            rank -= launchable.Name.IndexOf(input[0].ToString(), StringComparison.InvariantCultureIgnoreCase);
            if (launchable.Name.ContainsIgnoreCase(input)) rank += RankValue.NameContains;
            if (history.Contains(launchable)) rank += RankValue.HistoryContainsFile;
            if (history.ContainsAsSubstring(input, launchable)) rank += RankValue.HistoryContainsAsSubstring;
        }

        public static int operator -(FileRank x, FileRank y) {
            return x.rank - y.rank;
        }

        public override string ToString() {
            return rank.ToString();
        }

        private struct RankValue
        {
            internal const int NameContains = 04;
            internal const int HistoryContainsFile = NameContains*32;
            internal const int HistoryContainsAsSubstring = HistoryContainsFile*32;
            internal const int ExactMatch = HistoryContainsAsSubstring*32;
            internal const int PerfectMatch = ExactMatch*32;
        }
    }
    internal class FileRankContext {
        private readonly Dictionary<Launchable, FileRank> fileRanks = new Dictionary<Launchable, FileRank>();

        public FileRank Rank(Launchable launchable, History history, string input) {
            FileRank fileRank;
            if (fileRanks.TryGetValue(launchable, out fileRank)) return fileRank;
            fileRank = new FileRank(launchable, history, input);
            fileRanks.Add(launchable, fileRank);
            return fileRank;
        }
    }
}