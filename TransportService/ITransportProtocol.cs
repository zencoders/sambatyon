using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TransportService
{
    [ServiceContract]
    public interface ITransportProtocol
    {

        [OperationContract]
        ChunkResponse GetChunk(ChunkRequest chkrq);
    }

    [DataContract]
    public abstract class AbstractMessage
    {
        protected string messageType;
        protected int rID;
        protected int cID;

        [DataMember]
        public abstract string MessageType
        {
            get;
            set;
        }

        [DataMember]
        public abstract int RID
        {
            get;
            set;
        }

        [DataMember]
        public abstract int CID
        {
            get;
            set;
        }
    }

    [DataContract]
    public class ChunkRequest : AbstractMessage
    {
        new string messageType;
        int activeBuffer;

        public ChunkRequest() { }

        public ChunkRequest(int activeBuffer, int RID, int CID)
        {
            this.messageType = "CHKRQ";
            this.activeBuffer = activeBuffer;
            this.CID = CID;
            this.RID = RID;
        }

        [DataMember]
        public override string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        [DataMember]
        public override int RID
        {
            get { return rID; }
            set { rID = value; }
        }

        [DataMember]
        public override int CID
        {
            get { return cID; }
            set { cID = value; }
        }

        [DataMember]
        public int ActiveBuffer
        {
            get { return activeBuffer; }
            set { activeBuffer = value; }
        }
    }

    [DataContract]
    public class ChunkResponse : AbstractMessage
    {
        new string messageType;
        int servingBuffer;
        byte[] payload;

        public ChunkResponse() { }

        public ChunkResponse(int servingBuffer, int RID, int CID, byte[] payload)
        {
            this.messageType = "CHKRQ";
            this.servingBuffer = servingBuffer;
            this.RID = RID;
            this.CID = CID;
        }

        [DataMember]
        public override string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        [DataMember]
        public override int RID
        {
            get { return rID; }
            set { rID = value; }
        }

        [DataMember]
        public override int CID
        {
            get { return cID; }
            set { cID = value; }
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