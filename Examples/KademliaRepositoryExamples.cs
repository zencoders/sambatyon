using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Persistence;
using System.Reflection;
using Persistence.Tag;

namespace Examples
{
    class KademliaRepositoryExamples
    {
        private static KademliaRepository _repository=null;
        public static void RunExamples()
        {
            ExampleHelper.ExampleSetPrint("Kademlia Repository Examples", typeof(KademliaRepositoryExamples));
            RepositoryConfiguration conf=new RepositoryConfiguration(new {data_dir = @"..\..\Resource\Database"});
            _repository = new KademliaRepository("Raven", conf);
            StoreExample();
        }
        public static void StoreExample()
        {
            ExampleHelper.ExampleMethodPrint("Store a Tag and linking it with a peer URI", MethodInfo.GetCurrentMethod());
            CompleteTag tag = new CompleteTag(@"..\..\Resource\Garden.mp3");            
            _repository.StoreResource(tag, new Uri("http://localhost:18292"));
            CompleteTag anotherTag = new CompleteTag(@"..\..\Resource\SevenMP3.mp3");
            _repository.StoreResource(anotherTag,new Uri("http://localhost:28182"));
        }
    }
}
