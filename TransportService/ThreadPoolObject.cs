using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TransportService
{
    class ThreadPoolObject
    {
        private Thread chunkGetter;
        private AutoResetEvent threadLock;

        public ThreadPoolObject()
        {
            this.threadLock = new AutoResetEvent(true);
        }

        public AutoResetEvent ThreadLock
        {
            set { this.threadLock = value; }
            get { return this.threadLock; }
        }

        public void assignAndStart(ThreadStart d)
        {
            this.threadLock.WaitOne();
            this.threadLock.Reset();
            chunkGetter = new Thread(() => d());
            chunkGetter.Start();
            chunkGetter.Join();
            this.threadLock.Set();
        }
    }
}
