using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
namespace Tag {
    /// <summary>
    /// Classe rappresentante una versione alleggerita del Tag completo.
    /// La rappresentazione dei dati viene effettuata su un array di 128 byte e le stringhe vengono codificate in 
    /// ASCII. Questa versione alleggerita serve come chiave primaria negli scambi protocollari di Kademlia.
    /// Il tag leggero contiene informazioni sul titolo, l'artista e l'album ed è finalizzta al calcolo dell'affinità
    /// semantica ad una ricerca.
    /// <para>
    /// La struttura del Tag è composta da un header di 3 byte, ognuno dei quali contenente la lunghezza del campo associato, e 
    /// da un body di 125 byte che memorizza il contenuto; l'ordine in cui i campi vengono rappresentati è [titolo, artista, album]
    /// ossia vanno dal più importante (il titolo) al meno importante (l'album) durante la ricerca.    
    /// Per memorizzare i dati del Tag Completo si usa la funzione <see cref="Persistence.Tag.LightTag.SetTagData"/> che utilizza 
    /// opportune regole per il troncamento dei campi inseriti. Per dare precedenza al titolo si assicura il massimo spazio possibile 
    /// a tale campo; agli altri due campi vengono riservati al massimo 20 + 10 = 30 byte (se risultano più piccoli gli verrà riservato
    /// meno spazio). Se il titolo è più lungo dei caratteri disponibile verrà troncato. Il campo artista viene immagazzinato utilizzando
    /// tutto lo spazio disponibile tranne i 10 byte riservati all'album. Se non è possibile contenere tutta la stringa nei byte 
    /// disponibili viene eseguito un troncamento. Infine l'album occuperà ciò che rimane dell'array di byte.
    /// </para>
    /// </summary>
    public class LightTag
    {
        /// <summary>
        /// Vettore di byte contenente i dati grezzi del tag
        /// </summary>
        private byte[] _rawData;
        /// <summary>
        /// Costante che indica la dimensione dell'header in byte
        /// </summary>
        private const byte _contentOffset = 3;
        /// <summary>
        /// Costante che indica il numero di byte disponibili per il contenuto del tag.
        /// </summary>
        private const int _availableBytes = 128 - _contentOffset;
        /// <summary>
        /// Costante che indica il numero di byte riservati al campo artista 
        /// </summary>
        private const int _artistReservedBytes = 20;
        /// <summary>
        /// Costante che indica il numero di byte riservati al campo album
        /// </summary>
        private const int _albumReservedBytes = 10;
        /// <summary>
        /// Costruttore di default del Tag che alloca il vettore di byte e inizializza <b>ESPLICITAMENTE</b> i dati a 0.
        /// </summary>
        public LightTag()
        {
            this._rawData = new byte[128];
            //initialize array => using for loop because it's mush faster than Enumerable.repeat!
            for (int k = 0; k < this._rawData.Length; k++)
            {
                this._rawData[k] = 0;
            }
        }
        /// <summary>
        /// Costruttore del Tag che permette di copiare i dati, contenuti nel byte array passato, nel buffer del tag.
        /// </summary>
        /// <param name="data">Vettore di byte da cui copiare i dati.</param>
        public LightTag(byte[] data)
        {
            this._rawData = data.Clone() as byte[];
        }
        /// <summary>
        /// Costruttore che permette di estrarre un Tag alleggerito da un tag completo.
        /// </summary>
        /// <param name="tag">Tag Completo da cui estrarre i dati</param>
        public LightTag(CompleteTag tag):this()
        {
            this.SetTagData(tag.Title, tag.Artist, tag.Album);
        }
        /// <summary>
        /// Proprietà che rappresenta i dati grezzi contenuti nel Tag
        /// </summary>
        public byte[] RawData
        {
            get
            {
                return _rawData.Clone() as byte[];
            }
            protected set
            {
                this._rawData = value.Clone() as byte[];
            }
        }
        /// <summary>
        /// Proprietà che rappresenta il titolo associato al Tag
        /// </summary>
        public string Title
        {
            get
            {
                if (this._rawData[0] != 0)
                {
                    byte size = this._rawData[0];
                    byte offset = _contentOffset;
                    byte[] buffer=this._rawData.Skip(offset).Take(size).ToArray();                
                    ASCIIEncoding enc = new ASCIIEncoding();
                    return enc.GetString(buffer);
                }
                else
                {
                    return "";
                }
            }
            protected set
            {
                this._rawData[0] = (byte)value.Length;
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] strArray = enc.GetBytes(value);
                int offset = _contentOffset;
                for (int k = 0; k < strArray.Length; k++)
                {
                    this._rawData[k + offset] = strArray[k];
                }
            }
        }
        /// <summary>
        /// Proprietà che rappresenta l'artista associato al brano
        /// </summary>
        public string Artist
        {
            get
            {
                if (this._rawData[1] != 0)
                {
                    byte size = this._rawData[1];
                    int offset = _contentOffset + this._rawData[0];
                    byte[] buffer = this._rawData.Skip(offset).Take(size).ToArray();
                    ASCIIEncoding enc = new ASCIIEncoding();
                    return enc.GetString(buffer);
                }
                else
                {
                    return "";
                }
            }
            private set 
            {
                this._rawData[1] = (byte)value.Length;
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] strArray = enc.GetBytes(value);
                int offset = _contentOffset+this._rawData[0];
                for (int k = 0; k < strArray.Length; k++)
                {
                    this._rawData[k + offset] = strArray[k];
                }

            }
        }
        /// <summary>
        /// Proprietà che rappresenta l'album in cui è contenuto il brano
        /// </summary>
        public string Album
        {
            get
            {
                if (this._rawData[2] != 0)
                {
                    byte size = this._rawData[2];
                    int offset = _contentOffset + this._rawData[0] + this._rawData[1];
                    byte[] buffer = this._rawData.Skip(offset).Take(size).ToArray();
                    ASCIIEncoding enc = new ASCIIEncoding();
                    return enc.GetString(buffer);
                }
                else
                {
                    return "";
                }
            }
            private set
            {
                this._rawData[2] = (byte)value.Length;
                ASCIIEncoding enc = new ASCIIEncoding();
                byte[] strArray = enc.GetBytes(value);
                int offset = _contentOffset + this._rawData[0] + this._rawData[1];
                for (int k = 0; k < strArray.Length; k++)
                {
                    this._rawData[k + offset] = strArray[k];
                }
            }
        }
        /// <summary>
        /// Metodo utilizzato per impostare i campi all'interno del Tag.
        /// Questo metodo si occupa di troncare i campi troppo lunghi. La descrizione dettaglia è contenuta nella descrizione della classe <see cref="Persistence.Tag.LightTag"/>
        /// </summary>
        /// <param name="title">Il campo titolo da impostare</param>
        /// <param name="artist">Il campo artista da impostare</param>
        /// <param name="album">Il campo album da impostare</param>
        /// <returns>Un array contenente la dimensione dei vari campi inseriti</returns>
        public int[] SetTagData(string title, string artist, string album)
        {
            int necessarySize = title.Length + artist.Length + album.Length;
            if (necessarySize <= _availableBytes)
            {
                this.Title = title;
                this.Artist = artist;
                this.Album = album;
                return new int[3] { title.Length, artist.Length, album.Length };
            }
            else
            {
                //Give precedence to title
                int[] sizeArray = new int[3];
                int artistMinLenght = (artist.Length > _artistReservedBytes ? _artistReservedBytes  : artist.Length);
                int albumMinLenght = (album.Length > _albumReservedBytes ? _albumReservedBytes : album.Length);
                int otherLenght = artistMinLenght + albumMinLenght;
                int titleAvailableLenght=_availableBytes-otherLenght;
                int titleLenght = title.Length > titleAvailableLenght ? titleAvailableLenght : title.Length;
                this.Title = title.Substring(0, titleLenght);
                sizeArray[0]=titleLenght;
                int otherAvailableLenght = _availableBytes - titleLenght-albumMinLenght;
                int artistLength = artist.Length > otherAvailableLenght ? otherAvailableLenght : artist.Length;
                this.Artist = artist.Substring(0, artistLength);
                sizeArray[1]=artistLength;
                otherAvailableLenght = _availableBytes - titleLenght - artistLength;
                int albumLenght = album.Length > otherAvailableLenght? otherAvailableLenght : album.Length;
                this.Album = album.Substring(0, albumLenght);
                sizeArray[2] = albumLenght;
                return sizeArray;                
            }
        }
    }
}} // namespace Persistence.Tag
