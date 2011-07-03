using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using Persistence;

namespace PeerPlayer
{
    [ServiceContract]
    interface IPeer
    {
        [OperationContract]
        Stream ConnectToStream();

        [OperationContract]
        void RestartFlow();

        [OperationContract]
        void GetFlow(string RID, int begin, long length, Dictionary<string, float> nodes);

        [OperationContract]
        void StopFlow();

        [OperationContract]
        void Configure(string udpPort, string kademliaPort);

        [OperationContract]
        bool StoreFile(string filename);

        [OperationContract]
        IList<KademliaResource> SearchFor(string queryString);
    }
}
