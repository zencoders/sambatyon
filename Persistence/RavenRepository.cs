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
using Raven.Client.Client;
using Raven.Storage;
using Raven.Client;
using System.Transactions;
using Raven.Client.Document;
using log4net;
using System.Threading.Tasks;
using Raven.Database.Json;
using Raven.Database.Data;
using Newtonsoft.Json.Linq;
using Raven.Database.Indexing;
using Raven.Client.Indexes;
using System.Runtime.InteropServices;

namespace Persistence
{
namespace RepositoryImpl
{
    /// <summary>
    /// Repository implementation using RavenDB.
    /// </summary>
    class RavenRepository : Repository
    {
        /// <summary>
        /// Logger used for log
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(RavenRepository));
        /// <summary>
        /// Embedded Store used for persistence
        /// </summary>
        private EmbeddableDocumentStore _store;        
        //private DocumentStore _store;        
        /// <summary>
        /// Constructor of the repository. 
        /// The directory where the database will be located in the path indicated in the Configuration Field named <c>data_dir</c>. If 
        /// no data directory is specified a temporary path will be used.
        /// </summary>
        /// <param name="config">Configuration used to initialize the Repository</param>
        public RavenRepository(RepositoryConfiguration config=null)
        {
            log.Info("Trying to load RavenRepository class...");
            this.RepositoryType = "Raven";
            string dataDir = System.IO.Path.GetTempPath()+"/p2p-player-db";
            if (config != null)
            {
                string tdd = config.GetConfig("data_dir");
                if (!tdd.Equals("")) dataDir = tdd;
            }
            log.Debug("Start opening and initializing RavenDB");
            _store = new EmbeddableDocumentStore { DataDirectory = dataDir};
            //_store = new DocumentStore { Url = "http://localhost:8080" };
            _store.Initialize();               
            log.Info("RavenDB initialized at " + dataDir);
        }        
        /// <summary>
        /// Method that save the repository type of the object passed as argument inside the RavenDB embedded document.
        /// </summary>
        /// <param name="data">Object to store</param>
        /// <returns>Always RepositoryResponse.RepositorySuccess</returns>
        public override RepositoryResponse Save(ILoadable data)
        {
            /*try
            {*/
                lock (_store)
                {
                    /*using (TransactionScope tx = new TransactionScope())
                    {*/
                        using (IDocumentSession _session = _store.OpenSession())
                        {
                            dynamic entity = data.GetAsDatabaseType();
                            _session.Store(entity);
                            try
                            {
                                _session.SaveChanges();
                            }
                            catch (COMException comE)
                            {
                                log.Warn("Cannot save cause to interop error",comE);
                            }
                      //      tx.Complete();
                            log.Debug("Data saved with id " + entity.Id);
                        }
                    //}
                }
            /*}
            catch (TransactionAbortedException tae)
            {
                log.Error("Transaction aborted", tae);
                return RepositoryResponse.RepositoryTransactionAbort;
            }*/
            return RepositoryResponse.RepositorySuccess;
        }
        /// <summary>
        /// Method that delete an element of the repository.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="id">Identifier of the element to delete.</param>
        /// <returns>RepositoryResponse.RepositoryDelete if the element has been successfully deleted, 
        /// RepositoryResponse.RepositoryGenericError otherwise</returns>
        public override RepositoryResponse Delete<DBType>(string id)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    var entity = _session.Load<DBType>(id);
                    if (entity != null)
                    {
                        /*try
                        {
                            using (TransactionScope tx = new TransactionScope())
                            {*/
                                _session.Delete<DBType>(entity);
                                _session.SaveChanges();
                            //    tx.Complete();
                                log.Debug("Data with id " + id + " deleted");
                            //}
                            return RepositoryResponse.RepositoryDelete;
                        /*}
                        catch (TransactionAbortedException tae)
                        {
                            log.Error("Transaction Aborted", tae);
                            return RepositoryResponse.RepositoryGenericError;
                        }*/
                    }
                    else
                    {
                        return RepositoryResponse.RepositoryGenericError;
                    }
                }
            }
        }
        /// <summary>
        /// Deletes a bunch of elements in a single call.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="ids">Array containing the identifiers of the elements that have to be deleted</param>
        /// <returns>RepositoryResponse.RepositoryDelete if all the elements have been successfully deleted, 
        /// RepositoryResponse.RepositoryGenericError otherwise</returns>
        public override RepositoryResponse BulkDelete<DBType>(string[] ids)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    DBType[] ents = _session.Load<DBType>(ids);
                    /*try
                    {*/
                        using (TransactionScope tx = new TransactionScope())
                        {
                            foreach (DBType entity in ents)
                            {
                                _session.Delete<DBType>(entity);
                            }
                            _session.SaveChanges();
                            tx.Complete();
                        }
                        return RepositoryResponse.RepositoryDelete;
                    /*}
                    catch (TransactionAbortedException tae)
                    {
                        log.Error("Transaction Aborted", tae);
                        return RepositoryResponse.RepositoryGenericError;
                    }*/
                }
            }
        }
        /// <summary>
        /// Returns the number of elements of given type stored in the repository
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <returns>Number of elements</returns>
        public override int Count<DBType>()
        {
            using (IDocumentSession _session = _store.OpenSession())
            {
                return _session.Query<DBType>().Customize(x => { x.WaitForNonStaleResults(); }).Count();
            }
        }
        /// <summary>
        /// Gets element of the given type and identified by the string passed as arguments.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="id">Identifier of the element to retrieve</param>
        /// <param name="elem">Reference where the object will be loaded</param>
        /// <returns>RepositoryResponse.RepositoryLoad if the element has been successfully loaded,
        /// RepositoryResponse.RepositoryMissingKey if the element is not contained in the repository, 
        ///  RepositoryResponse.RepositoryGenericError otherwise.</returns>
        public override RepositoryResponse GetByKey<DBType>(string id, ILoadable elem)
        {

            DBType entity;
            using (IDocumentSession _session = _store.OpenSession())
            {
                entity = _session.Load<DBType>(id);
            }
            if (entity != null)
            {
                if (elem.LoadFromDatabaseType(entity))
                {
                    log.Debug("Data with key " + id + " loaded");
                    return RepositoryResponse.RepositoryLoad;
                }
                else
                {
                    return RepositoryResponse.RepositoryGenericError;
                }
            }
            else
            {
                return RepositoryResponse.RepositoryMissingKey;
            }
        }
        /// <summary>
        /// Loads all elements of a given type that satisfy the condition passed as argument.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="cond">Condition use to choose elemets to loaded</param>
        /// <param name="elems">List where elements will be loaded</param>
        /// <returns>Always RepositoryResponse.RepositoryLoad</returns>
        public override RepositoryResponse GetAllByCondition<DBType>(Func<DBType, bool> cond,List<DBType> elems)
        {            
            using (IDocumentSession _session = _store.OpenSession())
            {
                var results = _session.Query<DBType>().Customize(x=> {x.WaitForNonStaleResults();}).AsParallel().Where(cond);
                elems.AddRange(results);
            }
            return RepositoryResponse.RepositoryLoad;
        }
        /// <summary>
        /// Loads elements using an array of desired identifier.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="ids">Array of identifier of the elements we want to load</param>
        /// <param name="elems">List where elements will be loaded</param>
        /// <returns>Always RepositoryResponse.RepositoryLoad</returns>
        public override RepositoryResponse GetByKeySet<DBType>(string[] ids, List<DBType> elems)
        {
            if (elems != null)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    DBType[] ents = _session.Load<DBType>(ids);
                    elems.AddRange(ents);
                }
                return RepositoryResponse.RepositoryLoad;
            }
            else
            {
                return RepositoryResponse.RepositoryGenericError;
            }
        }
        /// <summary>
        /// Gets all elements of a given type stored in the repository
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="cont">Container where elements will be loaded</param>
        /// <returns>Always RepositoryResponse.RepositoryLoad;</returns>
        public override RepositoryResponse GetAll<DBType>(ICollection<DBType> cont)
        {
            if (cont!=null) {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    foreach (DBType t in _session.Query<DBType>().Customize(x => { x.WaitForNonStaleResults(); }))
                    {
                        cont.Add(t);
                    }
                }
                return RepositoryResponse.RepositoryLoad;
            } else {
                return RepositoryResponse.RepositoryGenericError;
            }
        }
        /// <summary>
        /// Creates an index with the given name using the query passed as arguent.
        /// </summary>
        /// <param name="indexName">Name of the index</param>
        /// <param name="indexQuery">Query that will be execute to create and mantain the index</param>
        /// <returns>Always RepositoryResponse.RepositorySuccess</returns>
        public override RepositoryResponse CreateIndex(string indexName, string indexQuery)
        {
            lock (_store)
            {
                IndexDefinition def = new IndexDefinition() { Map = indexQuery, Name = indexName };
                //_store.
                if (_store.DatabaseCommands.GetIndex(indexName) == null)
                {
                    log.Debug(_store.DatabaseCommands.PutIndex(indexName, def));
                }
            }
            return RepositoryResponse.RepositorySuccess;
        }
        /// <summary>
        /// Queries the repository using an index to get the element faster that with normal Load method.
        /// </summary>
        /// <typeparam name="DBType">Type of the element stored in the database. This is used to identify the collection</typeparam>
        /// <param name="indexName">Name of the index to query</param>
        /// <param name="query">Query to execute</param>
        /// <param name="elems">List where elements will be loaded</param>
        /// <returns>
        /// RepositoryResponse.RepositoryLoad if elements have been successfully loaded,
        /// RepositoryResponse.RepositoryMissingIndex if the index has not been found, 
        /// RepositoryResponse.RepositoryGenericError otherwise
        /// </returns>
        public override RepositoryResponse QueryOverIndex<DBType>(string indexName, string query,List<DBType> elems){
            if (elems == null) return RepositoryResponse.RepositoryGenericError;
            if (this._store.DatabaseCommands.GetIndex(indexName) == null) return RepositoryResponse.RepositoryMissingIndex;
            using (IDocumentSession _session = _store.OpenSession()) {
                var results = _session.Advanced.LuceneQuery<DBType>(indexName).WaitForNonStaleResults().Where(query);                                
                elems.AddRange(results);
            }
            return RepositoryResponse.RepositoryLoad;
        }
        /// <summary>
        /// Method used internally to patch the database.
        /// </summary>
        /// <param name="key">RavenDB key of the element to patch</param>
        /// <param name="propertyName">Name of the property to patch</param>
        /// <param name="value">Value used for executing patching command</param>
        /// <param name="type">Type of Patch Command</param>
        /// <returns>Always True</returns>
        private bool patchDatabase(string key, string propertyName, object value, PatchCommandType type)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] {
                        new PatchCommandData {
                            Key = key,                        
                            Patches = new [] {
                                generatePatchRequest(propertyName,value,type)
                            }
                        }
                    }
                    );
                }
            }
            return true;
        }
        /// <summary>
        /// Method used internally to generate value-based database patch request
        /// </summary>
        /// <param name="propertyName">Name of the property to patch</param>
        /// <param name="value">Value to use with patch command</param>
        /// <param name="type">Type of the patch command</param>
        /// <returns>The patch request generated</returns>
        private PatchRequest generatePatchRequest(string propertyName, object value, PatchCommandType type)
        {
            return new PatchRequest {
                Type = type,
                Name = propertyName,
                Value = JToken.FromObject(value)
            };
        }
        /// <summary>
        /// Method used internally to generate position-based database patch request
        /// </summary>
        /// <param name="propertyName">Name of the property to patch</param>
        /// <param name="pos">Position to use with the patch command</param>
        /// <param name="type">Type of the patch command</param>
        /// <returns>THe patch request that has been generated</returns>
        private PatchRequest generatePatchRequest(string propertyName, int pos, PatchCommandType type)
        {
            return new PatchRequest {
                Type = type,
                Name = propertyName,
                Position = pos,
                Value = null
            };
        }
        /// <summary>
        /// Method used internally to execute a bunch of Patch Request in a single call.
        /// </summary>
        /// <param name="key">RavenDB key that identify the element to patch</param>
        /// <param name="reqs">Array of patch requests to execute</param>
        /// <returns>Always true</returns>
        private bool mupliplePatchDatabase(string key,params PatchRequest[] reqs)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] 
                    {
                        new PatchCommandData 
                        {
                            Key = key,
                            Patches = reqs
                        }
                    }
                    );
                }
            }
            return true;
        }
        /// <summary>
        /// Method used internally to execute nested patch command
        /// </summary>
        /// <param name="key">RavenDB key that identify the element to patch</param>
        /// <param name="propertyName">Name of the property to patch</param>
        /// <param name="index">Index of the element to patch</param>
        /// <param name="reqs">Array of patch requests to execute</param>
        /// <returns></returns>
        private bool nestedPatchDatabase(string key,string propertyName, int index, params PatchRequest[] reqs)
        {
            lock (_store)
            {
                using (IDocumentSession _session = _store.OpenSession())
                {
                    Raven.Database.BatchResult[] res = _session.Advanced.DatabaseCommands.Batch(
                        new[] 
                    {
                        new PatchCommandData 
                        {
                            Key = key,
                            Patches = new [] 
                            {
                                new PatchRequest 
                                {
                                    Type = PatchCommandType.Modify,
                                    Name = propertyName,
                                    Position = index,
                                    Nested = reqs
                                }
                            }
                        }
                    }
                    );
                }
            }
            return true;
        }
        /// <summary>
        /// Adds an element into an array property of an object in the repository
        /// </summary>
        /// <param name="key">Identifier of the object to update</param>
        /// <param name="property">Array property where the element has to be added</param>
        /// <param name="element">Element to add</param>
        /// <returns>Always RepositoryResponse.RepositoryPatchAdd</returns>
        public override RepositoryResponse ArrayAddElement(string key, string property, object element)
        {
            patchDatabase(key, property, element, PatchCommandType.Add);
            return RepositoryResponse.RepositoryPatchAdd;
        }
        /// <summary>
        /// Removes an element from an array property of an object contained in the repository
        /// </summary>
        /// <param name="key">Identifier of the object to update</param>
        /// <param name="property">Array property where the element has to be deleted</param>
        /// <param name="value">Element to delete</param>
        /// <returns>Always RepositoryResponse.RepositoryPatchRemove</returns>
        public override RepositoryResponse ArrayRemoveElement(string key, string property, object value)
        {
            patchDatabase(key, property, value, PatchCommandType.Remove);
            return RepositoryResponse.RepositoryPatchRemove;
        }
        /// <summary>
        /// Removes one or more elements from an array property of an object contained in the repository. This method
        /// uses array index to identify the element to delete.
        /// </summary>
        /// <param name="key">Identifier of the object to update</param>
        /// <param name="property">Array property where the element has to be deleted</param>
        /// <param name="values">Array contaning the indexes of the elements that have to be deleted</param>
        /// <returns>Always RepositoryResponse.RepositoryPatchRemove</returns>
        public override RepositoryResponse ArrayRemoveByPosition(string key,string property,params object[] values)
        {
            PatchRequest[] reqs = new PatchRequest[values.Length];
            for (int k = 0; k < values.Length; k++)
            {
                reqs[k] = generatePatchRequest(property, values[k], PatchCommandType.Remove);
            }
            mupliplePatchDatabase(key, reqs);
            return RepositoryResponse.RepositoryPatchRemove;
        }
        /// <summary>
        /// Sets the value of a property of an element (identified by its position) contained in an array property of an object in 
        /// the repository. This method updates a second level property.
        /// </summary>
        /// <param name="key">Identifier of the object to update</param>
        /// <param name="property">Array property where the element has to be updated</param>
        /// <param name="index">Index of the element that has to be updated</param>
        /// <param name="obj_prop">Name of the property of the element</param>
        /// <param name="value">New value of the property</param>
        /// <returns>Always RepositoryResponse.RepositoryPatchModify</returns>
        public override RepositoryResponse ArraySetElement(string key, string property, int index, string obj_prop, object value)
        {
            PatchRequest req = generatePatchRequest(obj_prop, value, PatchCommandType.Set);
            nestedPatchDatabase(key, property, index, req);
            return RepositoryResponse.RepositoryPatchModify;
        }
        /// <summary>
        /// Sets the value of a property of an object contained in the repository. This method updates a first level property.
        /// </summary>
        /// <param name="key">Identifier of the object to update</param>
        /// <param name="property">Name of the property that has to be updated</param>
        /// <param name="newValue">New Value of the property</param>
        /// <returns>Always RepositoryResponse.RepositoryPatchSet</returns>       
        public override RepositoryResponse SetPropertyValue(string key, string property, object newValue)
        {
            patchDatabase(key, property, newValue, PatchCommandType.Set);
            return RepositoryResponse.RepositoryPatchSet;
        }

        #region IDisposable
        /// <summary>
        /// Disposes the RavenDB embedded document Store
        /// </summary>
        public override void Dispose()
        {
            lock (_store)
            {
                log.Debug("Disposing Raven Repository...");
                this._store.Dispose();
                log.Info("Raven Repository Disposed. Resource Released");
            }
        }

        #endregion
    }
}
}