using System.Reflection;
using System.Windows.Media;
using Azazel.FileSystem;

namespace Venus {
    public class PluginIconLoader {
        private IconExtractor iconExtractor;

        public ImageSource Icon(string name) {
            return Load(name, "ico");
        }

        public ImageSource Png(string name) {
            return Load(name, "png");
        }

        private ImageSource Load(string name, string extension) {
            iconExtractor = IconExtractor.Instance;
            return
                iconExtractor.Extract(
                    new File(Paths.Combine(new File(Assembly.GetExecutingAssembly().Location).ParentFolder.FullName, "plugins", name + "." + extension)));
        }
    }
}
