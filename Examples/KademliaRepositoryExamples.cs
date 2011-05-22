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
        private static CompleteTag tag = new CompleteTag(@"..\..\Resource\Garden.mp3");
        public static void RunExamples()
        {
            ExampleHelper.ExampleSetPrint("Kademlia Repository Examples", typeof(KademliaRepositoryExamples));
            RepositoryConfiguration conf=new RepositoryConfiguration(new {data_dir = @"..\..\Resource\Database"});
            _repository = new KademliaRepository("Raven", conf);
            CleanTagExample();
            StoreExample();
            PutExample();
            GetAllExample();
            MiscGetAndContainsExample();
            SearchExample();
            RefreshExample();
            ExpireExample();

            //DeleteExample();
        }

        private static void ExpireExample()
        {
            ExampleHelper.ExampleMethodPrint("Clean Expire Entity", MethodInfo.GetCurrentMethod());
            _repository.Expire();
        }

        private static void MiscGetAndContainsExample()
        {
            ExampleHelper.ExampleMethodPrint("Show how some minor function of Get and Contains works", MethodInfo.GetCurrentMethod());
            bool resp = _repository.ContainsTag(tag.TagHash);
            Console.WriteLine("Contains Tag " + tag.TagHash + " ? " + resp);
            resp = _repository.ContainsTag("jasdkasjds");
            Console.WriteLine("Contains Tag jasdkasjds ? " + resp);
            Uri url = new Uri("http://localhost:18292");
            resp = _repository.ContainsUrl(tag.TagHash, url);
            Console.WriteLine("Resource " + tag.TagHash + " contains Url http://localhost:18292 ? " + resp);
            DateTime pubTime = _repository.GetPublicationTime(tag.TagHash, url);
            Console.WriteLine("Publication Time for Url " + url.ToString() + " on Resource " + tag.TagHash + " is " + pubTime);
        }

        private static void PutExample()
        {
            ExampleHelper.ExampleMethodPrint("Put a new DhtElement in a Resource", MethodInfo.GetCurrentMethod());
            _repository.Put(tag.TagHash, new Uri("http://127.0.0.1:18181"), DateTime.Now.AddDays(-1).AddHours(-1));
        }

        private static void GetAllExample()
        {
            ExampleHelper.ExampleMethodPrint("Print all KademliaResource in the repository", MethodInfo.GetCurrentMethod());
            LinkedList<KademliaResource> coll = _repository.GetAllElements();
            foreach (KademliaResource res in coll)
            {
                ExampleHelper.DumpObjectProperties(res);
                Console.WriteLine();
            }
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
            _repository.StoreResource(tag, new Uri("http://localhost:18292"),DateTime.Now);
            CompleteTag anotherTag = new CompleteTag(@"..\..\Resource\SevenMP3.mp3");
            _repository.StoreResource(anotherTag,new Uri("http://localhost:28182"),DateTime.Now);
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
            Console.WriteLine("Delete Result: "+_repository.DeleteTag(tag.TagHash));
        }
        public static void RefreshExample()
        {
            ExampleHelper.ExampleMethodPrint("Refresh a previously loaded tag", MethodInfo.GetCurrentMethod());
            _repository.RefreshResource(tag.TagHash, new Uri("http://localhost:18292"), DateTime.Now.AddHours(1));
            KademliaResource rs = _repository.Get(tag.TagHash);
            ExampleHelper.DumpObjectProperties(rs);
        }
    }
}
