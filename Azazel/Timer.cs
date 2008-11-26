using System;
using System.Threading;
using System.Windows.Threading;

namespace Azazel {
    public class Timer {
        private readonly Dispatcher dispatcher;
        private readonly VoidDelegate voidDelegate;
        private bool running;
        private System.Threading.Timer timer = new System.Threading.Timer(delegate { });

        public Timer(VoidDelegate voidDelegate) : this(null, voidDelegate) {}

        public Timer(Dispatcher dispatcher, VoidDelegate voidDelegate) {
            this.dispatcher = dispatcher;
            this.voidDelegate = voidDelegate;
        }

        public void Start(double seconds) {
            Stop();
            timer = new System.Threading.Timer(delegate { InvokeDelegate(); }, null, TimeSpan.FromSeconds(seconds), TimeSpan.FromTicks(Timeout.Infinite));
            running = true;
        }

        private void InvokeDelegate() {
            if (dispatcher == null) voidDelegate();
            else dispatcher.BeginInvoke(DispatcherPriority.Normal, voidDelegate);
        }

        public void Stop() {
            if (running) timer.Dispose();
        }
    }
}