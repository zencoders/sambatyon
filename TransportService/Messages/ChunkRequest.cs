using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TransportService.Messages
{
    [DataContract]
    public class ChunkRequest : GenericMessage
    {
        new string messageType;
        //        int activeBuffer;

        public ChunkRequest() { }

        public ChunkRequest(/*int activeBuffer, */string RID, int CID, Uri SenderAddress)
        {
            this.messageType = "CHKRQ";
            //            this.activeBuffer = activeBuffer;
            this.CID = CID;
            this.RID = RID;
            this.SenderAddress = SenderAddress;
        }

        /*      [DataMember]
              public int ActiveBuffer
              {
                  get { return activeBuffer; }
                  set { activeBuffer = value; }
              }*/
    }
}
