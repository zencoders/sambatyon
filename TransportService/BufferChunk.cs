using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace TransportService
{
    internal class BufferChunk
    {
        private System.Timers.Timer timer;
        private condition actualCondition;
        public enum condition { CLEAN, DIRTY, DOWNLOADED };

        public byte[] Payload
        {
            get;
            set;
        }

        public condition ActualCondition
        {
            get
            {
                return this.actualCondition; 
            }

            set 
            {
                this.actualCondition = value;
                if (value == condition.DIRTY)
                {
                    this.timer = new System.Timers.Timer(3000);
                    this.timer.Enabled = true;
                    this.timer.Elapsed += new ElapsedEventHandler(this.cleaner);
                }
                else if (value == condition.CLEAN)
                {
                    if(this.timer != null)
                        this.timer.Enabled = false;
                }
            }
        }

        private void cleaner(object source, EventArgs e)
        {
            if (this.actualCondition == condition.DIRTY)
            {
                this.actualCondition = condition.CLEAN;
            }
        }

        public BufferChunk()
        {
            this.actualCondition = condition.CLEAN;
        }

        public BufferChunk(byte[] payload)
        {
            this.Payload = payload;
            this.actualCondition = condition.DOWNLOADED;
        }
    }
}
