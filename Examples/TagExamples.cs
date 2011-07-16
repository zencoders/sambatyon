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
using Persistence.Tag;
using System.Reflection;

namespace Examples
{
    public static class TagExamples
    {
        public static void RunExamples() {
            ExampleHelper.ExampleSetPrint("Tag Examples",typeof(TagExamples));
            //TagReadAndWrite();
            CompleteTagReadAndWrite();
            //CompleteToLightTag();
        }
        private static void CompleteToLightTag()
        {

            ExampleHelper.ExampleMethodPrint("Generating Light Tag from Complete Tag and Read it", MethodInfo.GetCurrentMethod());
            CompleteTag tag = new CompleteTag(@"..\..\Resource\SevenMP3.mp3");
            Console.WriteLine("Read info from Complete Tag");
            Console.WriteLine("Title: " + tag.Title);
            Console.WriteLine("Artist: " + tag.Artist);
            Console.WriteLine("Album: " + tag.Album);
            LightTag miniTag = new LightTag(tag);
            Console.WriteLine("Read info from Light Tag");
            Console.WriteLine("Title. " + miniTag.Title);
            Console.WriteLine("Artist: " + miniTag.Artist);
            Console.WriteLine("Album: " + miniTag.Album);
            Console.WriteLine("Raw Byte Length: " + miniTag.RawData.Length);
            Console.WriteLine("Raw Byte : " + miniTag.ToString());
        }
        private static void CompleteTagReadAndWrite()
        {
            ExampleHelper.ExampleMethodPrint("Generating Complete Tag From File and Read it",MethodInfo.GetCurrentMethod());
            CompleteTag tag = new CompleteTag(@"..\..\Resource\SevenMP3.mp3");
            Console.WriteLine("Hash: " + tag.FileHash);
            Console.WriteLine("Title: " + tag.Title);
            Console.WriteLine("Artist: " + tag.Artist);
            Console.WriteLine("Album: " + tag.Album);
            Console.WriteLine("Genre: " + tag.Genre);
            Console.WriteLine("Year: " + tag.Year);
            Console.WriteLine("Track: " + tag.Track);
            Console.WriteLine("Length: " + tag.Length);
            Console.WriteLine("File Size: " + tag.FileSize);
            Console.WriteLine("Channels: " + tag.Channels);
            Console.WriteLine("Bitrate: " + tag.Bitrate);
            Console.WriteLine("Sample Rate: " + tag.SampleRate);
            //Console.WriteLine("Specific Tag: " + tag.SpecificTag.GetType().FullName);            
        }
        private static void TagReadAndWrite()
        {
            ExampleHelper.ExampleMethodPrint("Tag Read and Write Example", MethodInfo.GetCurrentMethod());
            LightTag tag = new LightTag();
            int[] ret = tag.SetTagData(
                "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
                "123456789012345678901234567890",
                "123456789012345678901234567890");
            Console.WriteLine("Sizes of value set");
            foreach (int i in ret)
            {
                Console.Write(i);
                Console.Write(" ");
            }
            Console.WriteLine("\nValues read");
            Console.WriteLine(tag.Title);
            Console.WriteLine(tag.Artist);
            Console.WriteLine(tag.Album);
        }
    }
}
