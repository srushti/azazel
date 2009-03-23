using System;
using System.Threading;
using System.Windows.Threading;

namespace Azazel.Threading {
    public class Timer<T> {
        private readonly Dispatcher dispatcher;
        private readonly TimerDelegate<T> timerDelegate;
        private bool running;
        private Timer timer = new Timer(delegate { });
        private T arguments;

        public Timer(TimerDelegate<T> timerDelegate) : this(null, timerDelegate) {}

        public Timer(Dispatcher dispatcher, TimerDelegate<T> timerDelegate) {
            this.dispatcher = dispatcher;
            this.timerDelegate = timerDelegate;
        }

        public bool Running {
            get { return running; }
        }

        public void Start(double seconds, T arguments) {
            this.arguments = arguments;
            Stop();
            timer = new Timer(delegate { InvokeDelegate(); }, null, TimeSpan.FromSeconds(seconds), TimeSpan.FromTicks(Timeout.Infinite));
            running = true;
        }

        private void InvokeDelegate() {
            Stop();
            if (dispatcher == null) timerDelegate(arguments);
            else dispatcher.BeginInvoke(DispatcherPriority.Normal, timerDelegate);
        }

        public void Stop() {
            if (running) {
                timer.Dispose();
                running = false;
            }
        }

        public void ForceCall() {
            Stop();
            InvokeDelegate();
        }
    }

    public delegate void TimerDelegate<T>(T input);
}