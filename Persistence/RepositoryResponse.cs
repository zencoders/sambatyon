using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    /// <summary>
    /// Tipo enumerazione che definisce le diverse risposte delle operazioni sul repository
    /// </summary>
    public enum RepositoryResponse
    {
        /// <summary>
        /// Valore che identifica un generico successo nell'operazione.
        /// </summary>
        RepositorySuccess = 0,
        /// <summary>
        /// Valore che identifica un errore generico (del quale non si hanno informazioni) occorso durante l'esecuzione
        /// dell'operazione richiesta.
        /// </summary>
        RepositoryGenericError = -1,
        /// <summary>
        /// Valore che identifica un errore di connessione al Repository
        /// </summary>
        RepositoryConnectionError = -2,
        /// <summary>
        /// Valore che identifica un errore che ha generato l'abort di una transazione
        /// </summary>
        RepositoryTransactionAbort = -3,
        /// <summary>
        /// Valore che identifica l'esecuzione di un'operazione di aggiornamento.
        /// </summary>
        RepositoryUpdate = 1,
        /// <summary>
        /// Valore che identifica l'esecuzione di un'operazione di inserimento.
        /// </summary>
        RepositoryInsert = 2,
        /// <summary>
        /// Valore che identifica l'esecuzione di un'operazine di cancellazione.
        /// </summary>
        RepositoryDelete = 3,
        /// <summary>
        /// Valore che identifica l'esecuzione di un'operazione di caricamento dati.
        /// </summary>
        RepositoryLoad = 4,
        /// <summary>
        /// Valore che identifica un errore causato dalla mancanza di un elemento associato alla chiave richiesta
        /// all'interno del repository.
        /// </summary>
        RepositoryMissingKey = -10,
        /// <summary>
        /// Valore che identifica un errore causato dal tentativo di inserimento di un elemento identificato da una chiave
        /// già presente all'interno del repository.
        /// </summary>
        RepositoryDuplicateKey = -20,
        /// <summary>
        /// Valore che identifica il successo di un'operazione di Patch sul repository
        /// </summary>
        RepositoryPatchSuccess = 10,
        /// <summary>
        /// Valore che identifica l'aggiunta di un elemento ad un array in fase di Patch sul repository
        /// </summary>
        RepositoryPatchAdd = 11,
        /// <summary>
        /// Valore che identifica la modifica di una proprietà in fase di Patch del Repository
        /// </summary>
        RepositoryPatchSet = 12,
        RepositoryPatchRemove = 13,
        RepositoryMissingIndex=-11
    }
}
