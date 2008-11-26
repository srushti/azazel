using System.Collections;

namespace Azazel.Extensions {
    public static class StaticEnumerable {
        public static bool IsEmpty(this ICollection collection) {
            return collection == null || collection.Count == 0;
        }
    }
}