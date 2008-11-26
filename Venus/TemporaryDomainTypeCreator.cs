using System;
using System.Diagnostics;
using System.Reflection;
using Azazel.FileSystem;

namespace Venus {
    public class TemporaryDomainTypeCreator<T> : IDisposable where T : MarshalByRefObject{
        private static readonly AppDomainSetup setup = new AppDomainSetup();
        private readonly AppDomain domain;

        public TemporaryDomainTypeCreator() {
            //Debugger.Launch();
            setup.ApplicationBase = new File(Type.Assembly.Location).ParentFolder.FullName;
            domain = AppDomain.CreateDomain("SomethingElse", null, setup);
        }

        public T Create(params object[] arguments) {
            return
                (T)
                domain.CreateInstanceAndUnwrap(Type.Assembly.FullName, Type.FullName, false, BindingFlags.Default, null, arguments, null, new object[] {},
                                               null);
        }

        public Type Type {
            get { return typeof (T); }
        }

        public void Dispose() {
            AppDomain.Unload(domain);
        }
    }
}
