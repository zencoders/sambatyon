using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TransportService
{
    public class TransportProtocol : ITransportProtocol
    {

        public ChunkResponse GetChunk(ChunkRequest chkrq)
        {
            byte[] a = BitConverter.GetBytes(3678);
            return new ChunkResponse(10, chkrq.RID, chkrq.CID, a);
            //TODO: Implement the method
        }
    }

    public class Transferer
    {
        public ChunkResponse GetRemoteChunk(ChunkRequest chkrq, string address)
        {
            ITransportProtocol svc = ChannelFactory<ITransportProtocol>.CreateChannel(
                new NetTcpBinding(), new EndpointAddress(address)
            );
            ChunkResponse result = svc.GetChunk(chkrq);
            return result;
        }
    }
}