using System;
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
