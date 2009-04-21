using System.Reflection;
using Azazel.FileSystem;

namespace Azazel.Extensions {
    public static class StaticAssembly {
        public static string GetExecutingFolder(this Assembly assembly) {
            return new File(assembly.Location).ParentFolder.FullName;
        }
    }
}