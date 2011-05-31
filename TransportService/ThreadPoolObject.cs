using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TransportService
{
    public class ThreadPoolObject
    {
        public enum ThreadState { FREE, BUSY };
        private Thread threadWorker;
        private AutoResetEvent threadLock;
        private ThreadState state;

        public ThreadPoolObject()
        {
            this.threadLock = new AutoResetEvent(true);
        }

        public AutoResetEvent ThreadLock
        {
            set { this.threadLock = value; }
            get { return this.threadLock; }
        }

        public ThreadState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        public void assignAndStart(ThreadStart d)
        {
            this.state = ThreadState.BUSY;
            this.threadLock.WaitOne();
            this.threadLock.Reset();
            threadWorker = new Thread(() => d());
            threadWorker.Start();
            threadWorker.Join();
            this.state = ThreadState.FREE;
            this.threadLock.Set();
        }
    }
}
