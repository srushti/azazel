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

        public History(File historyFile, XStream xstream) {
            this.historyFile = historyFile;
            this.xstream = xstream;
            dictionary = PersistanceHelper.SaveOrLoadAndSave<Dictionary<string, Launchable>>(xstream, Paths.Instance.History, LoadHistory, StoreHistory,
                                                                                             "Because of some error (probably connected with plugins) all your history has been lost!!");
        }

        protected virtual string LoadHistory() {
            return File.Contents(Paths.Instance.History);
        }

        public Launchable this[string input] {
            get { return dictionary.ContainsKey(input) ? dictionary[input.ToLower()] : null; }
            set { dictionary.SetValue(input.ToLower(), value); }
        }

        protected virtual void StoreHistory() {
            historyFile.WriteAllText(xstream.ToXml(dictionary));
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