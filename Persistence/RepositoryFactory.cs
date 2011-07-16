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
using log4net;

namespace Persistence
{
    /// <summary>
    /// Classe Factory che si occupa della creazione dei Repository in modo da mascherare le classi specializzate.
    /// Le classi create per essere richiamate dalla Factory devono essere contenute nel namespace Persistence.RepositoryImpl e 
    /// devono chiamarsi <c><i>NomeTipo</i>Repository</c> e devono avere un attributo costante chiamato RepositoryType 
    /// contenente una stringa che identifica il tipo del repository.
    /// Il tipo del repository viene usato dalla Factory per generarne un'istanza.
    /// </summary>
    /// <seealso cref="Persistence.Repository"/>
    public static class RepositoryFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RepositoryFactory));
        /// <summary>
        /// Metodo privato che genera il nome della classe del Repository in base al suo tipo.
        /// </summary>
        /// <param name="repType">Il tipo di repository</param>
        /// <returns>La stringa contenente il nome completo della classe implementante il repository del tipo richiesto.</returns>
        private static String _generateRepositoryClassName(String repType) {
            return "Persistence.RepositoryImpl." + repType + "Repository";
            //[]
        }
        /// <summary>
        /// Metodo statico per ottenere un'istanza di un repository di un tipo specificato.
        /// Se tutto va a buon fine il metodo ritornerà un oggetto delle classe Persistence.RepositoryImpl.TipoRepository derivata
        /// dalla classe astratta Persistence.Repository.
        /// Se viene richiesto un tipo di repository non disponibile verrà sollevata un'eccezione; è quindi preferibile, nel caso in 
        /// cui non si sia sicuri della disponibilità di un tipo di repository, usare il metodo IsValidRepositoryType della Factory.
        /// </summary>
        /// <example>
        /// Persistence.Repository repo=Persistence.RepositoryFactory("raven");
        /// Console.Writeline(repo.RepositoryType); //raven
        /// </example>
        /// <param name="repType">Stringa rappresentante il tipo di repository richiesto.</param>
        /// <returns>Un'istanza del repository del tipo richiesto.</returns>
        /// <exception cref="System.TypeLoadException">Eccezione sollevata nel caso ci siano errori nel caricamento della classe 
        /// del repository o nell'invocazione del suo costruttore di default.</exception>
        /// <exception cref="System.ArgumentException">Eccezione sollevata nel caso in cui la stringa contenente il tipo di repository
        /// richiesto risulti vuota</exception>
        /// <exception cref="System.ArgumentNullException">Eccezione sollevata nel caso in cui la stringa contenente il tipo di
        /// repository richiesto risulti essere nulla</exception>
        public static Repository GetRepositoryInstance(String repType,RepositoryConfiguration config)
        {
            if (repType != null)
            {
                if (repType.Length != 0)
                {
                    String className = _generateRepositoryClassName(repType);
                    try
                    {
                        log.Debug("1");
                        Type reflectedRepository = Type.GetType(className, true, true);
                        log.Debug("2");
                        Type[] args = new Type[1] {config.GetType()};
                        log.Debug("3");
                        Object[] param = new Object[1] { config };
                        log.Debug("4");
                        object rep = reflectedRepository.GetConstructor(args).Invoke(param);
                        log.Debug("5");
                        Repository inst=rep as Repository;
                        log.Debug("6");
                        if (inst.RepositoryType.Equals(repType)) {
                            log.Debug("stella!");
                            return inst;
                        } else {
                            log.Debug("7");
                            throw new TypeLoadException("Type "+inst.RepositoryType+" is different from the expected "+repType+"!");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new TypeLoadException("Unable to load repository class " + className, ex);
                    }
                }
                else
                {
                    throw new ArgumentException("Repository Type Name must not be Empty!");
                }
            }
            else
            {
                throw new ArgumentNullException("Repository Type Name must not be NULL!");
            }
        }
        /// <summary>
        /// Metodo statico utilizzato per sapere se è disponibile una classe rappresentante il repository con il tipo specificato.
        /// </summary>
        /// <param name="repType">Tipo di repository richiesto</param>
        /// <returns>Ritorna <c>true</c> se il tipo di repository specificato esiste, altrimenti <c>false</c></returns>
        public static  bool IsValidRepositoryType(String repType)
        {
            if ((repType != null) && (repType.Length != 0))
            {
                String className = _generateRepositoryClassName(repType);
                Type reflectedRepository = Type.GetType(className, false, true);
                return (reflectedRepository != null) ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
}
