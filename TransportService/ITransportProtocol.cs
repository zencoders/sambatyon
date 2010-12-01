using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TransportService
{
    // NOTA: è possibile utilizzare il comando "Rinomina" del menu "Refactoring" per modificare il nome di interfaccia "IService1" nel codice e nel file di configurazione contemporaneamente.
    [ServiceContract]
    public interface ITransportProtocol
    {

        [OperationContract]
        string GetData(int value);
        [OperationContract]
        ChunkResponse GetChunk(ChunkRequest chkr);
        //        [OperationContract]
        //        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: aggiungere qui le operazioni del servizio
    }

    [DataContract]
    public abstract class AbstractMessage
    {
        protected string messageType;
        protected string rID;
        protected string cID;

        [DataMember]
        public abstract string MessageType
        {
            get;
            set;
        }

        [DataMember]
        public abstract string RID
        {
            get;
            set;
        }

        [DataMember]
        public abstract string CID
        {
            get;
            set;
        }
    }

    [DataContract]
    public class ChunkRequest : AbstractMessage
    {
        new string messageType = "CHKRQ";
        int activeBuffer;

        [DataMember]
        public override string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        [DataMember]
        public override string RID
        {
            get { return rID; }
            set { rID = value; }
        }

        [DataMember]
        public override string CID
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
        new string messageType = "CHKRQ";
        int servingBuffer;
        byte[] payload;

        [DataMember]
        public override string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

        [DataMember]
        public override string RID
        {
            get { return rID; }
            set { rID = value; }
        }

        [DataMember]
        public override string CID
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