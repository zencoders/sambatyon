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
using PeerPlayer;

namespace wpf_player
{
    public class LocalStoreModel:INotifyPropertyChanged
    {
        private Peer peer = null;
        private ObservableCollection<CompleteTag> coll=new ObservableCollection<CompleteTag>();
        public LocalStoreModel(Peer p = null)
        {
            peer = p;
            RefreshList = new AlwaysExecuteCommand(refreshList);
            refreshList();
        }
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
        public ICommand RefreshList{
            set; get;
        }
        public bool StoreFile(string filename)
        {
            return peer.StoreFile(filename);
        }
        private void refreshList(object sender = null)
        {
            if (coll!=null)
            {
                coll.Clear();
            }
            IList<TrackModel.Track> tl = peer.GetAllTracks();
            foreach (TrackModel.Track tk in tl)
            {
                coll.Add(new CompleteTag(tk.Filename));
            }
            NotifyPropertyChanged("LocalStore");
        }
            
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

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
