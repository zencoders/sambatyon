using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Persistence.Tag;

namespace Examples
{
    public static class TagExamples
    {
        public static void RunExamples() {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Tag Examples (TagExaples)");
            Console.ResetColor();
            TagReadAndWrite();
            CompleteTagReadAndWrite();
            CompleteToLightTag();
        }
        private static void CompleteToLightTag()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Generating Light Tag from Complete Tag and Read it (Examples.TagExamples.CompleteToLightTag)");
            Console.ResetColor();
            CompleteTag tag = new CompleteTag("..\\..\\Resource\\SevenMP3.mp3");
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
        }
        private static void CompleteTagReadAndWrite()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Generating Complete Tag From File and Read it (Examples.TagExamples.CompleteTagReadAndWrite)");
            Console.ResetColor();
            CompleteTag tag = new CompleteTag("..\\..\\Resource\\SevenMP3.mp3");
            Console.WriteLine("Hash: " + tag.Hash);
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
            Console.WriteLine("Specific Tag: " + tag.SpecificTag.GetType().FullName);            
        }
        private static void TagReadAndWrite()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Tag Read and Write Example (Examples.TagExamples.TagReadAndWrite)");
            Console.ResetColor();
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
