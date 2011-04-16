using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagLib.Mpeg;
using System.IO;
using System.Security.Cryptography;

namespace Persistence
{
namespace Tag
{
    public class CompleteTag
    {
        public CompleteTag() { }
        public CompleteTag(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            this.Hash= sb.ToString();
            this.FillTag(filename);
        }

        private bool _fillTagMpeg(string filename)
        {
            using (AudioFile mpegFile = new AudioFile(filename))
            {
                TagLib.Tag fileTag = mpegFile.GetTag(TagLib.TagTypes.Id3v2, false);
                if (fileTag == null)
                {
                    mpegFile.GetTag(TagLib.TagTypes.Id3v1, false);
                }
                if (fileTag != null)
                {
                    this.Title = fileTag.Title;
                    this.Album = fileTag.Album;
                    this.Artist = fileTag.Performers.FirstOrDefault();
                    this.Genre = fileTag.Genres.FirstOrDefault();
                    this.Track = fileTag.Track;
                    this.Year = fileTag.Year;
                }
                else
                {
                    return false;
                }
                this.SpecificTag = fileTag;
                AudioHeader header;
                if (AudioHeader.Find(out header, mpegFile, 0))
                {
                    this.Bitrate = header.AudioBitrate;
                    this.Length = mpegFile.Properties.Duration.Seconds;
                    this.Channels = header.AudioChannels;
                    this.SampleRate = header.AudioSampleRate;
                }
                else
                {
                    return false;
                }
                FileInfo fi = new FileInfo(filename);
                this.FileSize = fi.Length;
            }
            return true;
        }

        public void FillTag(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                bool done=false;
                done=done||this._fillTagMpeg(filename); //Use shortcircuit to avoid call if done is true
                if (!done)
                {
                    throw new NotImplementedException("File type not supported");
                }
            }
            else
            {
                throw new System.IO.FileNotFoundException("File existence check failed.", filename);
            }            
        }
        #region "Properties"
        public string Title
        {
            get;
            set;
        }
        public string Artist
        {
            get;
            set;
        }
        public string Album
        {
            get;
            set;
        }
        public string Genre
        {
            get;
            set;
        }
        public uint Year
        {
            get;
            set;
        }
        public uint Track
        {
            get;
            set;
        }
        public int Length
        {
            get;
            set;
        }
        public int Bitrate
        {
            get;
            set;
        }
        public int SampleRate
        {
            get;
            set;
        }
        public int Channels
        {
            get;
            set;
        }
        public long FileSize
        {
            get;
            set;
        }
        public Object SpecificTag
        {
            get;
            set;
        }
        public string Hash
        {
            get;
            set;
        }
        #endregion        
    }       
}} //namespace Persistence.Tag
