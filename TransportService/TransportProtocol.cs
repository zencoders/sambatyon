using System;

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
}