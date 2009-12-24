using System;
using System.Collections.Generic;

namespace Azazel {
    public static class Arrays {
        public static T[] Create<T>(this List<T> list) {
            var array = Array.CreateInstance(typeof (T), list.Count);
            for (int i = 0; i < list.Count; i++)
                array.SetValue(list[i], i);
            return (T[]) array;
        }
    }
}