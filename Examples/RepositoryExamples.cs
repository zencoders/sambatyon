using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Persistence;

namespace Examples
{
    class RepositoryExamples
    {
        private static string _hid;
        private static Persistence.Repository<TrackModel.Track> _trackRep;
        public static void RunExamples()
        {
            ExampleHelper.ExampleSetPrint("Repository Examples", typeof(RepositoryExamples));
            LoadRavenExample();
            StoreTrackInDb();
            LoadTrackFromDb();
            InsertCountLoadAllDeleteAndLoadAgain();
            _trackRep.Dispose();
        }
        public static void LoadRavenExample()
        {
            ExampleHelper.ExampleMethodPrint("Generate Raven Repository Instance", MethodInfo.GetCurrentMethod());            
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(new { data_dir = "..\\..\\Resource\\Database" });
            _trackRep= Persistence.RepositoryFactory.GetRepositoryInstance<TrackModel.Track>("Raven",conf);
            Console.WriteLine(_trackRep.GetType().FullName+" - " + _trackRep.RepositoryType);
        }
        public static void StoreTrackInDb()
        {
            ExampleHelper.ExampleMethodPrint("Create a TrackModel from file and store it in the database",MethodInfo.GetCurrentMethod());
            TrackModel track = new TrackModel("..\\..\\Resource\\SevenMP3.mp3");
            _hid = track.GetAsDatabaseType().Id;
            ExampleHelper.DumpObjectProperties(track.GetAsDatabaseType());
            Console.WriteLine("Response : "+_trackRep.Save(track));
        }
        public static void LoadTrackFromDb() {
            ExampleHelper.ExampleMethodPrint("Load a TrackModel from the database",MethodInfo.GetCurrentMethod());
            TrackModel track= new TrackModel();
            RepositoryResponse resp = _trackRep.GetByKey(_hid, track);
            Console.WriteLine("Response : "+resp);
            if (resp>=0) {
                ExampleHelper.DumpObjectProperties(track.GetAsDatabaseType());
            }
        }
        public static void InsertCountLoadAllDeleteAndLoadAgain()
        {
            ExampleHelper.ExampleMethodPrint("Insert a new Track in the Database, Count all elements and the Load it all!\n"+
                                                "Then delete a item and Load it all again", MethodInfo.GetCurrentMethod());
            TrackModel track = new TrackModel("..\\..\\Resource\\Garden.mp3");
            ExampleHelper.DumpObjectProperties(track.GetAsDatabaseType());
            Console.WriteLine("Save Response : " + _trackRep.Save(track));
            Console.WriteLine("Count : " + _trackRep.Count());
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            Console.WriteLine("GetAll Response : " + _trackRep.GetAll(list));
            foreach (TrackModel.Track t in list)
            {
                ExampleHelper.DumpObjectProperties(t);
            }
            TrackModel anotherTrack = new TrackModel();           
            Console.WriteLine("Delete Response: "+ _trackRep.Delete(list.First().Id));
            list.Clear();
            Console.WriteLine("GetAll Response : " + _trackRep.GetAll(list));
            foreach (TrackModel.Track t in list)
            {
                ExampleHelper.DumpObjectProperties(t);
            }
        }
    }
}
