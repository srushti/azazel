using System.Collections.Generic;
using System.Windows.Media;

namespace Azazel.FileSystem {
    public interface Launchable {
        /// <summary>
        /// name that will be displayed on the main window and will be searched against
        /// </summary>
        string Name { get; }

        ImageSource Icon { get; }

        /// <summary>
        /// custom action available on this Launchable. return empty collection if there aren't any
        /// </summary>
        Actions Actions { get; }

        void Launch(string arguments);
        bool ShouldStoreHistory { get; }
    }

    public interface Action {
        void Act();
        string Name { get; }
    }

    public class Actions : List<Action> {
        public Actions() {}
        public Actions(IEnumerable<Action> collection) : base(collection) {}
        public Actions(params Action[] actions) : base(actions) {}
    }
}