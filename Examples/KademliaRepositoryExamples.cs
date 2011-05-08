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
            CleanTagExample();
            StoreExample();
            SearchExample();
            DeleteExample();
        }
        public static void CleanTagExample()
        {
            ExampleHelper.ExampleMethodPrint("Clean a string from semanticless words", MethodInfo.GetCurrentMethod());
            string example1 = "The wind of change";
            Console.WriteLine("\""+example1+ "\" => \""+_repository.DiscardSemanticlessWords(example1)+"\"");
        }
        public static void StoreExample()
        {
            ExampleHelper.ExampleMethodPrint("Store a Tag and linking it with a peer URI", MethodInfo.GetCurrentMethod());
            CompleteTag tag = new CompleteTag(@"..\..\Resource\Garden.mp3");            
            _repository.StoreResource(tag, new Uri("http://localhost:18292"));
            CompleteTag anotherTag = new CompleteTag(@"..\..\Resource\SevenMP3.mp3");
            _repository.StoreResource(anotherTag,new Uri("http://localhost:28182"));
        }
        public static void SearchExample()
        {
            ExampleHelper.ExampleMethodPrint("Trying to Search for Tags with a search Query", MethodInfo.GetCurrentMethod());
            string query = "cross";
            KademliaResource[] results = _repository.SearchFor(query);
            foreach (KademliaResource rs in results)
            {
                ExampleHelper.DumpObjectProperties(rs);
            }
        }
        public static void DeleteExample()
        {
            ExampleHelper.ExampleMethodPrint("Delete a previously loaded tag", MethodInfo.GetCurrentMethod());
            CompleteTag tag = new CompleteTag(@"..\..\Resource\Garden.mp3");
            Console.WriteLine("Delete Result: "+_repository.DeleteTag(tag.TagHash));
        }
    }
}
