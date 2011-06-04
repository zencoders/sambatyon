using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransportService
{
    internal class ThreadPool
    {
        private ThreadPoolObject[] pool;
        private int poolSize;

        public ThreadPool()
        {
        }

        public ThreadPool(int poolSize)
        {
            this.poolSize = poolSize;
            this.pool = new ThreadPoolObject[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                this.pool[i] = new ThreadPoolObject();
            }
        }

        public ThreadPoolObject GetNextThreadInPool()
        {
            return this.pool.AsParallel().Aggregate((l, r) => l.State == ThreadPoolObject.ThreadState.FREE ? l : r);
        }
    }
}
