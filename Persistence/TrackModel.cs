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

namespace Persistence {
    /// <summary>
    /// Classe rappresentante il Modello per una traccia.
    /// </summary>
    public class TrackModel : ILoadable
    {
        /*#region Attributes
        /// <summary>
        /// Id del brano all'interno del DB. In genere si tratta del checksum MD5 del file
        /// </summary>
        private String _id;
        /// <summary>
        /// Titolo del brano
        /// </summary>
        private String _title;
        /// <summary>
        /// Autore del brano
        /// </summary>
        private String _author;
        /// <summary>
        /// Album all'interno del quale si trova la traccia
        /// </summary>
        private String _album;
        /// <summary>
        /// Anno, sotto forma di stringa, del brano
        /// </summary>
        private String _year;
        /// <summary>
        /// Path del file contenente il brano.
        /// </summary>
        private String _filename;
        #endregion
        #region Properties
        /// <summary>
        /// Titolo del brano
        /// </summary>
        public String Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }
        /// <summary>
        /// Autore del brano
        /// </summary>
        public String Author
        {
            get
            {
                return this._author;
            }
            set
            {
                this._author = value;
            }
        }
        /// <summary>
        /// Album all'interno del quale si trova la traccia
        /// </summary>
        public String Album
        {
            get
            {
                return this._album;
            }
            set
            {
                this._album = value;
            }
        }
        /// <summary>
        /// Anno, sotto forma di stringa, del brano
        /// </summary>
        public String Year
        {
            get
            {
                return this._year;
            }
            set
            {
                this._year = value;
            }
        }
        /// <summary>
        /// Id del brano all'interno del DB. In genere si tratta del checksum MD5 del file
        /// </summary>
        public String Id {
            get
            { 
                return this._id;
            }
        }
        /// <summary>
        /// Path del file contenente il brano.
        /// </summary>
        public String Filename{
            get
            {
                return this._filename;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Costruttore di default del modello. Associa a tutti gli attributi il valore Stringa vuote.
        /// </summary>
        private TrackModel():this("", "", "", "", "", "") {
        }
        /// <summary>
        /// Costruttore base del modello. Viene lasciato privato lasciando al metodi statici il compito di fare validazione
        /// ed in seguito di istanziare gli oggetti TrackModel.
        /// </summary>
        /// <param name="id">Id della traccia all'interno del DB</param>
        /// <param name="title">Titolo del Brano</param>
        /// <param name="author">Artista del Brano</param>
        /// <param name="album">Album che contiene il Brano</param>
        /// <param name="year">Anno del Brano</param>
        /// <param name="filename">Path del File contenente il Brano</param>
        private TrackModel(String id, String title, String author, String album, String year, String filename)
        {
            this._id = id;
            this._title = title;
            this._album = album;
            this._year = year;
            this._filename = filename;
        }
        #endregion
        #region Static Members
        /// <summary>
        /// Metodo statico per la creazione di una nuova traccia a partire da un vettore di stringhe contenenti i dati 
        /// estratti dal database (o da altre sorgenti dati).
        /// I dati contenuti nell'Array devono essere i seguenti (è importante anche l'ordine): id,titolo,artista,album,anno,path.
        /// </summary>
        /// <param name="data">Array di Stringhe contenente i dati per generare il modello.</param>
        /// <returns>L'oggetto TrackModel creato dai dati oppure null se i dati sono insufficienti o errati</returns>
        public static TrackModel LoadFromData(String[] data) {
            TrackRepositoryFactory.GetRepositoryInstance("pippo");
            if (data.Length<6) {
                return null;
            } else {
                return new TrackModel(data[0],data[1],data[2],data[3],data[4],data[5]);
            }
        }
        /// <summary>
        /// Metodo statico per la crezione di una nuova traccia a partire da un file. I dati vengono letti dal file MPEG e vengono
        /// inseriti nel modello.
        /// </summary>
        /// <param name="filename">Il path del file da cui leggere i dati</param>
        /// <returns>L'oggetto TrackModel creato leggendo il file oppure null se il file non esiste oppure non contiene dati.</returns>
        public static TrackModel LoadFromFile(String filename) {
            if (System.IO.File.Exists(filename)) {
                AudioFile mpegFile = new AudioFile(filename);
                TagLib.Tag fileTag = mpegFile.GetTag(TagLib.TagTypes.Id3v2, false);
                if (fileTag != null) {
                    TagLib.Id3v2.Tag tagv3 = fileTag as TagLib.Id3v2.Tag;
                    FileStream file= new FileStream(filename,FileMode.Open);
                    MD5 md5= new MD5CryptoServiceProvider();
                    byte[] retVal=md5.ComputeHash(file);
                    file.Close();
                    StringBuilder sb=new StringBuilder();
                    for (int i=0;i<retVal.Length;i++) {
                        sb.Append(retVal[i].ToString("x2"));
                    }                    
                    String id=sb.ToString();
                    TrackModel newTrack = new TrackModel(id, tagv3.Title,tagv3.FirstPerformer, tagv3.Album,
                                                            Convert.ToString(tagv3.Year), filename);
                    return newTrack;
                } else {
                    TrackModel newTrack = new TrackModel();
                    return newTrack;
                }
            } else {            
                return null;
            }
        }
        #endregion
         */
        public TrackModel()
        {
            this.Tag = new Tag.CompleteTag();
            this.Filepath = "";
        }
        public TrackModel(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                FileInfo finfo = new FileInfo(filename);
                this.Tag = new Tag.CompleteTag(finfo.FullName);
                this.Filepath = finfo.FullName;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        public string Hash
        {
            get
            {
                return Tag.FileHash;
            }
        }
        public string Filepath
        {
            get;
            private set;
        }
        public Tag.CompleteTag Tag {
            get;
            private set;
        }


        #region ILoadable

        public class Track:IDocumentType
        {
            public string Id
            {
                get;
                set;
            }
            public string Filename
            {
                get;
                set;
            }
        }
        public dynamic GetAsDatabaseType()
        {
            return new Track
            {
                Id = this.Hash,
                Filename = this.Filepath
            };
        }

        public bool LoadFromDatabaseType(dynamic data)
        {
            Tag.CompleteTag t = new Tag.CompleteTag(data.Filename);
            if (t.FileHash.Equals(data.Id))
            {
                this.Tag = t;
                this.Filepath = data.Filename;
                return true;
            }
            else
            {
                return false;
            }
        }

        public Type GetDatabaseType()
        {
            return typeof(Track);
        }



        #endregion
        
    }
}
