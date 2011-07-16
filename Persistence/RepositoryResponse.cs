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
        RepositoryMissingIndex = -11,
        RepositoryPatchModify = 14
    }
}
