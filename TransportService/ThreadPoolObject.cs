using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TransportService
{
    internal class ThreadPoolObject
    {
        public enum ThreadState { FREE, BUSY };

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

        private void assignAndStartDelegate(Delegate d)
        {
            d.DynamicInvoke();
            this.State = ThreadState.FREE;
            this.ThreadLock.Set();
        }

        public void AssignAndStart(Delegate d)
        {
            this.State = ThreadState.BUSY;
            this.ThreadLock.WaitOne();
            this.ThreadLock.Reset();
            Thread t = new Thread(new ThreadStart(() => assignAndStartDelegate(d)));
            t.Start();
        }
    }
}
