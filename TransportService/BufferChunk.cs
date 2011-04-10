using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TransportService
{
    class BufferChunk
    {
        public enum condition { CLEAN, DIRTY, DOWNLOADED };
        private byte[] payload;
        private condition actualCondition;

        public byte[] Payload
        {
            get { return this.payload; }
            set { this.payload = value; }
        }

        public condition ActualCondition
        {
            get { return this.actualCondition; }
            set { this.actualCondition = value; }
        }

        public BufferChunk()
        {
            this.actualCondition = condition.CLEAN;
        }

        public BufferChunk(byte[] payload)
        {
            this.payload = payload;
            this.actualCondition = condition.DOWNLOADED;
        }
    }
}
