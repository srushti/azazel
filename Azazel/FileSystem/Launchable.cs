using System.Collections.Generic;
using System.Windows.Media;

namespace Azazel.FileSystem {
    public interface Launchable {
        /// <summary>
        /// name that will be displayed on the main window and will be searched against
        /// </summary>
        string Name { get; }

        /// <summary>
        /// image to be used in the display
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// custom action available on this Launchable. return empty collection if there aren't any
        /// </summary>
        Actions Actions { get; }

        /// <summary>
        /// method called to execute this application
        /// </summary>
        /// <param name="arguments">additional optional arguments</param>
        void Launch(string arguments);

        /// <summary>
        /// used to determine whether the execution of this instance needs to be stored in history
        /// </summary>
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