using System;
using System.Windows.Threading;

namespace Azazel.Threading {
    public class Thread {
        private readonly System.Threading.Thread thread;

        public Thread(VoidDelegate voidDelegate) : this(new System.Threading.Thread(() => voidDelegate())) {}

        public Thread(ParameterisedDelegate parameterisedDelegate, object parameter)
            : this(new System.Threading.Thread(() => parameterisedDelegate(parameter))) {}

        public Thread(System.Threading.Thread thread) {
            this.thread = thread;
        }

        public static Thread CurrentThread {
            get { return new Thread(System.Threading.Thread.CurrentThread); }
        }

        public void Start() {
            thread.Start();
        }

        public static void Sleep(TimeSpan timeSpan) {
            System.Threading.Thread.Sleep(timeSpan);
        }

        public void Abort() {
            thread.Abort();
        }
    }

    public static class StaticThread {
        public static Thread Thread(this Dispatcher dispatcher) {
            return new Thread(dispatcher.Thread);
        }
    }

    public delegate void ParameterisedDelegate(object parameter);
}