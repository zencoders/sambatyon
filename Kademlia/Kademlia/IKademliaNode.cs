using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kademlia.Messages;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Kademlia
{
    [ServiceContract]
    public interface IKademliaNode
    {
        [OperationContract]
        void HandleMessage(Message msg);

        [OperationContract]
        void CacheResponse(Response response);

        [OperationContract]
        void HandlePing(Ping ping);

        [OperationContract]
        void HandlePong(Pong pong);

        [OperationContract]
        void HandleFindNode(FindNode request);

        [OperationContract]
        void HandleFindNodeResponse(FindNodeResponse response);

        [OperationContract]
        void HandleFindValue(FindValue request);

        [OperationContract]
        void HandleFindValueDataResponse(FindValueDataResponse response);

        [OperationContract]
        void HandleFindValueContactResponse(FindValueContactResponse response);

        [OperationContract]
        void HandleStoreQuery(StoreQuery request);

        [OperationContract]
        void HandleStoreResponse(StoreResponse response);

        [OperationContract]
        void HandleStoreData(StoreData request);
    }
}
