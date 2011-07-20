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
using System.ComponentModel;
using Persistence.Tag;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Persistence;
using PeerLibrary;

namespace wpf_player
{
    /// <summary>
    /// Class that implements the ViewModel in the MVVM pattern. This class is used to access peer functionality related
    /// to the track repository
    /// </summary>
    public class LocalStoreModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Local reference to the Peer
        /// </summary>
        private Peer peer = null;
        /// <summary>
        /// Observable collection containing the resources
        /// </summary>
        private ObservableCollection<CompleteTag> coll=new ObservableCollection<CompleteTag>();
        /// <summary>
        /// Constructor that initializes the commands and the local reference to the peer
        /// </summary>
        /// <param name="p">Peer that will be used to get information</param>
        public LocalStoreModel(Peer p = null)
        {
            peer = p;
            RefreshList = new AlwaysExecuteCommand(refreshList);
            refreshList();
        }
        /// <summary>
        /// List containing all Resource Tag cointained in the track repository
        /// </summary>
        public IList<CompleteTag> LocalStore
        {
            get
            {
                return coll;
            }
            set
            {
                coll = new ObservableCollection<CompleteTag>(value);
                NotifyPropertyChanged("LocalStore");
            }
        }
        /// <summary>
        /// Refresh List command
        /// </summary>
        public ICommand RefreshList{
            set; get;
        }
        /// <summary>
        /// Method used to store a new track in the repository
        /// </summary>
        /// <param name="filename">Filename of the file to add</param>
        /// <returns>True if the track has been successfully added, false otherwise</returns>
        public bool StoreFile(string filename)
        {
            return peer.StoreFile(filename);
        }
        /// <summary>
        /// Backend method for the Refresh List Command. This method cleans the local list, reloads all data from the database and 
        /// then it updates the list.
        /// </summary>
        /// <param name="sender">Unused param</param>
        private void refreshList(object sender = null)
        {
            if (coll!=null)
            {
                coll.Clear();
            }
            IList<TrackModel.Track> tl = peer.GetAllTracks();
            if (tl != null)
            {
                foreach (TrackModel.Track tk in tl)
                {
                    if (tk != null)
                        coll.Add(new CompleteTag(tk.Filename));
                }
            }
            NotifyPropertyChanged("LocalStore");
        }
            
        #region INotifyPropertyChanged
        /// <summary>
        /// Event for the property changed (this is used by WPF Data Binding)
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Method called when a property has been changed. This method rises the event PropertyChange.
        /// </summary>
        /// <param name="info">Name of the property that has been changed</param>
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
