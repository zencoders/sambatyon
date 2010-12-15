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
}