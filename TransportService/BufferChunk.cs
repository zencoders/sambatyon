using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransportService
{
    class BufferChunk
    {
        public enum condition { CLEAN, DIRTY, DOWNLOADED };

        public byte[] Payload
        {
            get;
            set;
        }

        public condition ActualCondition
        {
            get;
            set;
        }

        public BufferChunk()
        {
            this.ActualCondition = condition.CLEAN;
        }

        public BufferChunk(byte[] payload)
        {
            this.Payload = payload;
            this.ActualCondition = condition.DOWNLOADED;
        }
    }
}
