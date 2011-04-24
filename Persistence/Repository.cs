using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    /// <summary>
    /// Classe astratta rappresentante un generico Repository.
    /// Questa classe viene usata per schermare le diverse implementazioni del repository attraverso un pattern Factory.
    /// Per istanziare una classe derivata da Repository usare il metodo statico GetRepositoryInstance della classe 
    /// <see cref="Persistence.TrackRepositoryFactory"/>.
    /// </summary>
    /// 
    public abstract class Repository: IDisposable  {
        /// <summary>
        /// Metodo che si occupa del salvataggio delle informazioni all'interno del repository.
        /// Se l'elemento non esiste viene inserito, altrimenti viene soltanto aggiornato.
        /// </summary>
        /// <param name="data">Dato da salvare nel repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        /// <seealso cref="Persistence.RepositoryResponse"/>
        public abstract RepositoryResponse Save(ILoadable data);
        /// <summary>
        /// Metodo che si occupa di eliminare un elemento dal repository.
        /// </summary>
        /// <param name="id">Chiave identificativa dell'elemento che si vuole eliminare</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        public abstract RepositoryResponse Delete<DBType>(string id) where DBType : IDocumentType;
        /// <summary>
        /// Metodo che si occupa di eliminare un gruppo di elementi dal repository
        /// </summary>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        /// <param name="ids">Chiavi identificative degli elementi da eliminare</param>
        /// <returns></returns>
        public abstract RepositoryResponse BulkDelete<DBType>(string[] ids) where DBType : IDocumentType;
        /// <summary>
        /// Metodo che ritorna la dimensione del repository.
        /// </summary>
        /// <returns>La dimensione del repository.</returns>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        public abstract int Count<DBType>() where DBType : IDocumentType;
        /// <summary>
        /// Metodo che si occupa di ritornare un elemento del repository identificato da una chiave. Il risultato viene scritto
        /// nel parametro <c>elem</c> passato in ingresso
        /// </summary>
        /// <param name="id">Chiave identificativa dell'elemento che si vuole ricercare</param>
        /// <param name="elem">Parametro di riferimento in uscita che verrà valorizzato con i dati provenienti dal repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        public abstract RepositoryResponse GetByKey<DBType>(string id, ILoadable elem) where DBType : IDocumentType;
        /// <summary>
        /// Metodo che si occupa di ritornare gli elementi del repository che soddisfano una determinata condizione.
        /// </summary>
        /// <typeparam name="DBType">Tipo di Dato usato nel database</typeparam>
        /// <param name="cond">Condizione che gli elementi devono soddisfare</param>
        /// <param name="elems">Lista a cui verranno aggiunti gli elementi trovari</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse GetAllByCondition<DBType>(Func<DBType, bool> cond,List<DBType> elems) where DBType: IDocumentType;
        /// <summary>
        /// Metodo che si occupa di ritornare gli elementid del repository identificati dalle chiavi fornite.
        /// </summary>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        /// <param name="ids">Array contenente le chiavi identificative degli elementi che si voglioni ricercare</param>
        /// <param name="elems">Parametro di riferimento in uscita che verrà valorizzato con i dati proveniente dal repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse GetByKeySet<DBType>(string[] ids, List<DBType> elems) where DBType: IDocumentType;
        /// <summary>
        /// Metodo che si occupa di ritornare tutti gli elementi del repository inserendoli in una collezione passata come parametro.
        /// </summary>
        /// <param name="cont">Collezione di ILoadable che verrà riempita con gli elementi estratti dal Repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        /// <typeparam name="DBType">Tipo di dato usato nel database</typeparam>
        public abstract RepositoryResponse GetAll<DBType>(ICollection<DBType> cont) where DBType : IDocumentType;
        /// <summary>
        /// Metodo per creare un indice sul repository
        /// </summary>
        /// <param name="indexName">Nome dell'indice da creare</param>
        /// <param name="indexQuery">Query che costituisce L'indice/param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse CreateIndex(string indexName, string indexQuery);
        /// <summary>
        /// Metodo per interrogare un indice
        /// </summary>
        /// <typeparam name="DBType">Tipo di dato usato nel repository</typeparam>
        /// <param name="indexName">Nome dell'indice da interrogare</param>
        /// <param name="query">Query</param>
        /// <param name="elems">Elementi dell'indice che soddisfano la condizione</param>
        /// <returns></returns>
        public abstract RepositoryResponse QueryOverIndex<DBType>(string indexName, string query, List<DBType> elems) where DBType:IDocumentType;
        /// <summary>
        /// Metodo che si occupa di aggiungere un elemento ad una proprietà di tipo array contenuta in un documento.
        /// Questo metodo evita il processo di update dell'intero documento.
        /// </summary>
        /// <param name="key">Chiave del documento da modificare</param>
        /// <param name="property">Proprietà da modificare</param>
        /// <param name="element">Elemento da aggiungere all'array</param>
        /// <returns>Il risultato dell'operazione sul repository. RItorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse ArrayAddElement(string key, string property, object element);
        /// <summary>
        /// Metodo che si occupa di rimuovere un elemento da una proprietà di tipo array contenuta in un documento.
        /// Questo metodo evita il processo di update dell'intero documento.
        /// </summary>
        /// <param name="key">Chiave del documento da modificare</param>
        /// <param name="property">Proprietà da modificare</param>
        /// <param name="index">Indice dell'elemento da rimuovere</param>
        /// <returns>Il risultato dell'operazione sul repository. RItorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse ArrayRemoveElement(string key, string property, object value);
        /// <summary>
        /// Metodo che si occupa di impostare il valore di una proprietà all'interno di un documento. 
        /// Questo metodo evita il processo di update dell'intero documento.
        /// </summary>
        /// <param name="key">Chiave del documento da modificare</param>
        /// <param name="property">Proprietà da modificare</param>
        /// <param name="newValue">Nuovo valore della proprietà</param>
        /// <returns>Il risultato dell'operazione sul repository. RItorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita</returns>
        public abstract RepositoryResponse SetPropertyValue(string key, string property, object newValue);

        public String RepositoryType
        {
            get;
            protected set;
        }
        public abstract void Dispose();
    }
}
