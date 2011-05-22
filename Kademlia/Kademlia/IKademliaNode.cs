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
        [OperationContract(IsOneWay = true)]
        void HandleMessage(Message msg);

        [OperationContract(IsOneWay = true)]
        void CacheResponse(Response response);

        [OperationContract(IsOneWay = true)]
        void HandlePing(Ping ping);

        [OperationContract(IsOneWay = true)]
        void HandlePong(Pong pong);

        [OperationContract(IsOneWay = true)]
        void HandleFindNode(FindNode request);

        [OperationContract(IsOneWay = true)]
        void HandleFindNodeResponse(FindNodeResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleFindValue(FindValue request);

        [OperationContract(IsOneWay = true)]
        void HandleFindValueDataResponse(FindValueDataResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleFindValueContactResponse(FindValueContactResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleStoreQuery(StoreQuery request);

        [OperationContract(IsOneWay = true)]
        void HandleStoreResponse(StoreResponse response);

        [OperationContract(IsOneWay = true)]
        void HandleStoreData(StoreData request);
    }
}
