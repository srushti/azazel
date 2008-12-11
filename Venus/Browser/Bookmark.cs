using System;
using System.Windows.Media;
using System.Xml.Serialization;
using Azazel.FileSystem;

namespace Venus.Browser {
    public class Bookmark : Launchable, IEquatable<Bookmark> {
        [XmlIgnore] private readonly ImageSource icon;
        private readonly string name;
        private readonly string url;

        protected Bookmark() {}

        public Bookmark(string name, string url, ImageSource icon) {
            this.name = name;
            this.url = url;
            this.icon = icon;
        }

        public string FullName {
            get { return url; }
        }

        public string Name {
            get { return name; }
        }

        public ImageSource Icon {
            get { return icon; }
        }

        public virtual void Launch(string arguments) {
            Launch(FullName, arguments);
        }

        public bool ShouldStoreHistory {
            get { return true; }
        }

        protected static void Launch(string fullName, string arguments) {
            new Runner(UrlLauncher.CommandToExecute(fullName, arguments)).AsyncStart();
        }

        public Actions Actions {
            get { return new Actions(); }
        }

        public override string ToString() {
            return string.Format("name : {0} url : {1}", name, url);
        }

        public bool Equals(Bookmark bookmark) {
            if (bookmark == null) return false;
            return Equals(name, bookmark.name) && Equals(url, bookmark.url);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Bookmark);
        }

        public override int GetHashCode() {
            return name.GetHashCode() + 29*url.GetHashCode();
        }
    }
}