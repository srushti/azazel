using System;

namespace Azazel {
    public class Thread {
        private readonly System.Threading.Thread thread;

        public Thread(VoidDelegate voidDelegate) {
            thread = new System.Threading.Thread(delegate() { voidDelegate(); });
        }

        public void Start() {
            thread.Start();
        }

        public static void Sleep(TimeSpan timeSpan) {
            System.Threading.Thread.Sleep(timeSpan);
        }
    }
}