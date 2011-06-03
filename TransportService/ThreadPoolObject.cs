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

        public ThreadPoolObject()
        {
            this.ThreadLock = new AutoResetEvent(true);
        }

        public AutoResetEvent ThreadLock
        {
            set;
            get;
        }

        public ThreadState State
        {
            get;
            set;
        }

        public void AssignAndStart(ThreadStart d)
        {
            this.State = ThreadState.BUSY;
            this.ThreadLock.WaitOne();
            this.ThreadLock.Reset();
            threadWorker = new Thread(() => d());
            threadWorker.Start();
            threadWorker.Join();
            this.State = ThreadState.FREE;
            this.ThreadLock.Set();
        }
    }
}
