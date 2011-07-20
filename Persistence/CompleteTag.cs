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
using TagLib.Mpeg;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace Persistence
{
namespace Tag
{
    /// <summary>
    /// This class contains all information need by the system to describe a Track. 
    /// The complete tag contains semantic information and audio information of the Track.
    /// Each Tag is identified by a TagHash which is the SHA1 of string containing author, title, album and year information
    /// about track. Thanks to this, one or more different files (with different hash of the content) can have the same TagHash:
    /// in other words different files that contains the same song have diffent file hash but same tag hash and so Kademlia Knowledge
    /// sharing can be tag oriented and not file oriented !
    /// </summary>
    [DataContractAttribute]    
    public class CompleteTag
    {
        /// <summary>
        /// Tag Default constructor.
        /// </summary>
        public CompleteTag() { }
        /// <summary>
        /// Main constructor of the Tag. This reads information from the given filename and initializes all fields of the tag.
        /// It's important to know that this constructor has a considerable weight in term of processor time because it calculates two SHA1 hash:
        /// one for the entire file and one for the relevant tag information.
        /// </summary>
        /// <param name="filename">Filename from whom extract the information for the tag</param>
        public CompleteTag(string filename)
        {
            byte[] retVal;
            SHA1 crypto = new SHA1CryptoServiceProvider();
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                retVal = crypto.ComputeHash(file);
                file.Close();
            }
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
        /// <summary>
        /// Method used for Serialization purpose
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
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

        /// <summary>
        /// Return a string representation of the relevant information of the tag.
        /// This method is used to construct the TagHash and its output is given as input for the SHA1 Hashing function.
        /// </summary>
        /// <returns>A String containing relevant information</returns>
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
        /// <summary>
        /// Reads information from a MPEG file and fills the tag fields.
        /// </summary>
        /// <param name="filename">Filename from w</param>
        /// <returns>Filename from whom extract the information for the tag</returns>
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
        /// <summary>
        /// Public method used to fill the tag.
        /// This relies on private specific method for each type of audio file. Currently only MPEG files are supported.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Thrown if the file type is not supported</exception>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the file does not exist</exception>
        /// <param name="filename">Filename from whom extract the information for the tag</param>
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
        /// <summary>
        /// Track Title
        /// </summary>
        [DataMemberAttribute]
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// Track Artist
        /// </summary>
        [DataMemberAttribute]
        public string Artist
        {
            get;
            set;
        }
        /// <summary>
        /// Album that contains the track
        /// </summary>
        [DataMemberAttribute]
        public string Album
        {
            get;
            set;
        }
        /// <summary>
        /// Genre of the the track
        /// </summary>
        [DataMemberAttribute]
        public string Genre
        {
            get;
            set;
        }
        /// <summary>
        /// Recording Year of the track
        /// </summary>
        [DataMemberAttribute]
        public uint Year
        {
            get;
            set;
        }
        /// <summary>
        /// Track Number 
        /// </summary>
        [DataMemberAttribute]
        public uint Track
        {
            get;
            set;
        }
        /// <summary>
        /// Track lenght in seconds
        /// </summary>
        [DataMemberAttribute]
        public int Length
        {
            get;
            set;
        }
        /// <summary>
        /// Track Bit Rate in Kbps (Kb per second)
        /// </summary>
        [DataMemberAttribute]
        public int Bitrate
        {
            get;
            set;
        }
        /// <summary>
        /// Sample rate in Hertz
        /// </summary>
        [DataMemberAttribute]
        public int SampleRate
        {
            get;
            set;
        }
        /// <summary>
        /// Number of channels
        /// </summary>
        [DataMemberAttribute]
        public int Channels
        {
            get;
            set;
        }
        /// <summary>
        /// Filesize in Byte
        /// </summary>
        [DataMemberAttribute]
        public long FileSize
        {
            get;
            set;
        }
        /// <summary>
        /// SHA1 hash of the file content
        /// </summary>
        [DataMemberAttribute]
        public string FileHash
        {
            get;
            set;
        }
        /// <summary>
        /// SHA1 hash of the relevant information of the tag (obtained with <see cref="contentString"/>
        /// </summary>
        [DataMemberAttribute]
        public string TagHash
        {
            get;
            set;
        }
        #endregion        
        
        /// <summary>
        /// Method used for serialization purpose.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
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
