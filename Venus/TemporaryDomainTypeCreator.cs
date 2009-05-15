using System;
using System.Reflection;
using System.Windows.Threading;
using Azazel.FileSystem;
using Azazel.Logging;

namespace Venus {
    public class TemporaryDomainTypeCreator<T> : IDisposable where T : MarshalByRefObject {
        private static readonly AppDomainSetup setup = new AppDomainSetup();
        private readonly AppDomain domain;

        public TemporaryDomainTypeCreator() {
            //Debugger.Launch();
            setup.ApplicationBase = new File(Type.Assembly.Location).ParentFolder.FullName;
            domain = AppDomain.CreateDomain("SomethingElse", null, setup);
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args) {
                                                           LogManager.WriteLog("trying to load " + args.Name);
                                                           return Assembly.Load(args.Name);
                                                       };
        }

        public T Create(params object[] arguments) {
            try {
                return
                    (T)
                    domain.CreateInstanceAndUnwrap(Type.Assembly.FullName, Type.FullName, false, BindingFlags.Default, null, arguments, null, new object[] {},
                                                   null);
            }
            catch (Exception exception) {
                LogManager.WriteLog(exception);
                throw;
            }
        }

        private static Type Type {
            get { return typeof (T); }
        }

        public void Dispose() {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            AppDomain.Unload(domain);
        }
    }
}