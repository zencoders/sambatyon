using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TransportService.Messages
{
    [DataContract]
    public class ChunkResponse : GenericMessage
    {
        int servingBuffer;
        byte[] payload;

        public ChunkResponse() { }

        public ChunkResponse(int servingBuffer, string RID, int CID, byte[] payload, Uri SenderAddress)
        {
            this.messageType = "CHKRQ";
            this.servingBuffer = servingBuffer;
            this.RID = RID;
            this.CID = CID;
            this.SenderAddress = SenderAddress;
        }

        [DataMember]
        public int ServingBuffer
        {
            get { return servingBuffer; }
            set { servingBuffer = value; }
        }

        [DataMember]
        public byte[] Payload
        {
            get { return payload; }
            set { payload = value; }
        }
    }
}
