using System;
using System.Collections.Generic;
using Azazel.Extensions;
using Azazel.FileSystem;

namespace Azazel {
    public class LaunchablesDictionary : Dictionary<Type, Launchables> {
        public void SetValue(Type key, Launchables value) {
            StaticDictionary.SetValue(this, key, value);
        }

        public Launchables Launchables {
            get {
                var launchables = new Launchables();
                foreach (var value in Values) launchables.AddRange(value);
                return launchables;
            }
        }
    }
}