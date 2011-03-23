using System;
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
    public class TrackModel
    {
        #region Attributes
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
        public String title
        {
            get
            {
                return this._title;
            }
        }
        public String author
        {
            get
            {
                return this._author;
            }
        }
        public String album
        {
            get
            {
                return this._album;
            }
        }
        public String year
        {
            get
            {
                return this._year;
            }
        }
        public String id {
            get
            { 
                return this._id;
            }
        }
        public String filname{
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
        static TrackModel loadFromData(String[] data) {
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
        static TrackModel loadFromFile(String filename) {
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
    }
}
