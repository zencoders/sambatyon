using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagLib.Mpeg;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace Persistence
{
namespace Tag
{
    public class CompleteTag : ISerializable
    {
        public CompleteTag() { }
        public CompleteTag(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Open);
            SHA1 crypto = new SHA1CryptoServiceProvider();
            byte[] retVal = crypto.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            this.FileHash= sb.ToString();            
            this.FillTag(filename);    
            System.Text.UTF8Encoding enc= new UTF8Encoding();
            byte[] tHashByte = crypto.ComputeHash(enc.GetBytes(this.contentString()));
            crypto.ComputeHash(tHashByte);
            sb.Clear();
            for (int i = 0; i < tHashByte.Length; i++)
            {
                sb.Append(tHashByte[i].ToString("x2"));
            }
            this.TagHash = sb.ToString();
        }
        public CompleteTag(SerializationInfo info, StreamingContext ctxt)
        {
            this.Title = (string) info.GetValue("Title", typeof(string));
            this.Artist = (string) info.GetValue("Artist", typeof(string));
            this.Album = (string) info.GetValue("Album", typeof(string));
            this.Genre = (string) info.GetValue("Genre", typeof(string));
            this.Year = (uint) info.GetValue("Year", typeof(uint));
            this.Track = (uint) info.GetValue("Track", typeof(uint));
            this.Length = (int) info.GetValue("Length", typeof(int));
            this.Bitrate = (int) info.GetValue("Bitrate", typeof(int));
            this.SampleRate = (int) info.GetValue("SampleRate", typeof(int));
            this.Channels = (int) info.GetValue("Channels", typeof(int));
            this.FileSize = (int) info.GetValue("FileSize", typeof(int));
            this.FileHash = (string) info.GetValue("FileHash", typeof(string));
            this.TagHash = (string) info.GetValue("TagHash", typeof(string));
        }

        private string contentString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Title.ToLower());
            sb.Append("#");
            sb.Append(this.Artist.ToLower());
            sb.Append("#");
            sb.Append(this.Album.ToLower());
            sb.Append("#");
            sb.Append(this.Year);
            return sb.ToString();
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
                AudioHeader header;
                if (AudioHeader.Find(out header, mpegFile, 0))
                {
                    this.Bitrate = header.AudioBitrate;
                    this.Length = (int)mpegFile.Properties.Duration.TotalSeconds;
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
                throw new System.IO.FileNotFoundException("File "+filename+" existence check failed.");
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
        public string FileHash
        {
            get;
            set;
        }
        public string TagHash
        {
            get;
            set;
        }
        #endregion        
    
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Title", this.Title);
            info.AddValue("Artist", this.Artist);
            info.AddValue("Album", this.Album);
            info.AddValue("Genre", this.Genre);
            info.AddValue("Year", this.Year);
            info.AddValue("Track", this.Track);
            info.AddValue("Length", this.Length);
            info.AddValue("Bitrate", this.Bitrate);
            info.AddValue("SampleRate", this.SampleRate);
            info.AddValue("Channels", this.Channels);
            info.AddValue("FileSize", this.FileSize);
            info.AddValue("FileHash", this.FileHash);
            info.AddValue("TagHash", this.TagHash);
        }
    }       
}} //namespace Persistence.Tag
