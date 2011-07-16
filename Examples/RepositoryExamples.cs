/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿using System;
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
        private static Persistence.Repository _trackRep;
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
            _trackRep= Persistence.RepositoryFactory.GetRepositoryInstance("Raven",conf);
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
            RepositoryResponse resp = _trackRep.GetByKey<TrackModel.Track>(_hid, track);
            Console.WriteLine("Response : "+resp);
            if (resp>=0) {
                ExampleHelper.DumpObjectProperties(track.GetAsDatabaseType());
            }
        }
        public static void InsertCountLoadAllDeleteAndLoadAgain()
        {
            ExampleHelper.ExampleMethodPrint("Insert a new Track in the Database, Count all elements and the Load it all!\n"+
                                                "Then delete a item and Load it all again", MethodInfo.GetCurrentMethod());
            TrackModel track = new TrackModel(@"..\..\Resource\Garden.mp3");
            ExampleHelper.DumpObjectProperties(track.GetAsDatabaseType());
            Console.WriteLine("Save Response : " + _trackRep.Save(track));
            Console.WriteLine("Count : " + _trackRep.Count<TrackModel.Track>());            
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            Console.WriteLine("GetAll Response : " + _trackRep.GetAll(list));
            foreach (TrackModel.Track t in list)
            {
                ExampleHelper.DumpObjectProperties(t);
            }
            TrackModel anotherTrack = new TrackModel();
            Console.WriteLine("Delete Response: " + _trackRep.Delete<TrackModel.Track>(list.First().Id));
            list.Clear();
            Console.WriteLine("GetAll Response : " + _trackRep.GetAll(list));
            foreach (TrackModel.Track t in list)
            {
                ExampleHelper.DumpObjectProperties(t);
            }
        }
    }
}
