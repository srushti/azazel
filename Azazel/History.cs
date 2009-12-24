using System.Collections.Generic;
using Azazel.Extensions;
using Azazel.FileSystem;
using xstream;

namespace Azazel {
    public class History {
        private readonly Dictionary<string, Launchable> dictionary = new Dictionary<string, Launchable>();
        private readonly File historyFile;
        private readonly XStream xstream;

        protected History() {}

        public History(File historyFile, XStream xstream, PersistanceHelper persistanceHelper) {
            this.historyFile = historyFile;
            this.xstream = xstream;
            dictionary = persistanceHelper.LoadOrSaveAndLoad(Paths.Instance.History, dictionary);
        }

        public Launchable this[string input] {
            get { return dictionary.ContainsKey(input) ? dictionary[input.ToLower()] : null; }
            set { dictionary.SetValue(input.ToLower(), value); }
        }

        protected virtual void StoreHistory() {
            PersistanceHelper.Save(historyFile.FullName, xstream, dictionary);
        }

        public void Add(string input, Launchable launchable) {
            if (!launchable.ShouldStoreHistory) return;
            if (File.Null.Equals(launchable)) return;
            dictionary[input.ToLower()] = launchable;
            StoreHistory();
        }

        public bool Contains(Launchable file) {
            return dictionary.ContainsValue(file);
        }

        public bool ContainsAsSubstring(string input, Launchable file) {
            foreach (var pair in dictionary)
                if (pair.Value.Equals(file) && pair.Key.Contains(input.ToLower())) return true;
            return false;
        }

        public bool IsExactMatch(string input, Launchable file) {
            Launchable historicalFile;
            return dictionary.TryGetValue(input.ToLower(), out historicalFile) && file.Equals(historicalFile);
        }
    }
}