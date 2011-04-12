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
    public abstract class Repository {
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
        /// <param name="elem">Elemento da eliminare</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        public abstract RepositoryResponse Delete(ILoadable elem);
        /// <summary>
        /// Metodo che ritorna la dimensione del repository.
        /// </summary>
        /// <returns>La dimensione del repository.</returns>
        public abstract int count();
        /// <summary>
        /// Metodo che si occupa di ritornare un elemento del repository identificato da una chiave. Il risultato viene scritto
        /// nel parametro <c>elem</c> passato in ingresso
        /// </summary>
        /// <param name="id">Chiave identificativa dell'elemento che si vuole ricercare</param>
        /// <param name="elem">Parametro di riferimento in uscita che verrà valorizzato con i dati provenienti dal repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        public abstract RepositoryResponse GetByKey(String id, out ILoadable elem);
        /// <summary>
        /// Metodo che si occupa di ritornare tutti gli elementi del repository inserendoli in una collezione passata come parametro.
        /// </summary>
        /// <param name="cont">Collezione di ILoadable che verrà riempita con gli elementi estratti dal Repository</param>
        /// <returns>Il risultato dell'operazione sul repository. Ritorna un valore negativo in caso di errori, altrimenti un valore
        /// che identifica l'operazione eseguita.</returns>
        public abstract RepositoryResponse GetAll(out ICollection<ILoadable> cont);

        public String RepositoryType
        {
            get;
            protected set;
        } 
    }
}
