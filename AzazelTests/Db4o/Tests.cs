using System;
using System.Collections.Generic;
using Azazel.FileSystem;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using NUnit.Framework;

namespace Azazel.Db4o {
    [TestFixture]
    public class Tests {
        [Test]
        public void MethodName() {
            var db4oFile = Paths.Instance.Db4o;
            File.Delete(db4oFile);
            var objectContainer = Db4oFactory.OpenFile(db4oFile);
            objectContainer.Store(new File(@"C:\Users\sgopalak\Documents\filethatdoesntexist"));
            objectContainer.Close();
            objectContainer = Db4oFactory.OpenFile(db4oFile);
            var result = (from Launchable l in objectContainer where l.Name.Contains("doesnt") select l );
            Console.WriteLine(result.Count());
            foreach (var launchable in result) {
                Console.WriteLine(launchable.Name);
            }
            var launchables = objectContainer.Query((Launchable launchable) => launchable.Name.Contains("doesnt"));
            Console.WriteLine(launchables.Count);
            foreach (var launchable in launchables) {
                Console.WriteLine(launchable.Name);
            }
        }
    }
}