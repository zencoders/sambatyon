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
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Persistence;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PeerLibrary;

namespace wpf_player
{
    /// <summary>
    /// Class that implements the ViewModel in the MVVM pattern. This class contains logic for handling the research and the
    /// comminication with other viewmodels
    /// </summary>
	public class SearchListModel : INotifyPropertyChanged
	{
        /// <summary>
        /// Observable collaction containing search response
        /// </summary>
        private ObservableCollection<KademliaResource> responseList=new ObservableCollection<KademliaResource>();
        /// <summary>
        /// Delegate for event handler for the streaming requested event
        /// </summary>
        /// <param name="sender">Object that sends the events</param>
        /// <param name="args">Event arguments</param>
        public delegate void StreamRequestedEventHandler(object sender, StreamRequestedArgs args);
        /// <summary>
        /// Stream requested event. This is used by the audio player to trigger the resource handling.
        /// </summary>
        public event StreamRequestedEventHandler StreamRequested;
        /// <summary>
        /// Local reference to the Peer
        /// </summary>
        private Peer peer = null;
        /// <summary>
        /// Query string 
        /// </summary>
        private string queryStr;
        /// <summary>
        /// Constructor that initializes the commands and the local reference to the peer
        /// </summary>
        /// <param name="peer">Peer that will be used to get information from the network</param>
		public SearchListModel(Peer peer=null)
		{
            this.peer = peer;
            Query = "";            
            Search = new AlwaysExecuteCommand(searchFn);
            RaiseStartStream = new AlwaysExecuteCommand(startStream);            
        }
        #region Property
        /// <summary>
        /// List containing the resources that have been found by the Peer during the research phase
        /// </summary>
        public IList<KademliaResource> QueryResponse
        {
            get
            {
                return responseList;
            }
        }
        /// <summary>
        /// String representing the query used for research
        /// </summary>
        public string Query
        {
            get
            {
                return queryStr;
            }
            set
            {
                queryStr = value;
                NotifyPropertyChanged("Query");
            }
        }
        #endregion

        #region Command
        /// <summary>
        /// Command used to raise a start stream event
        /// IMPORTANT: This command is not used anymore
        /// </summary>
        public ICommand RaiseStartStream
        {
            get;
            private set;
        }
        /// <summary>
        /// Command used to perform search
        /// </summary>
        public ICommand Search
        {
            get;
            private set;
        }
        #endregion
        #region Command Backfunction
        /// <summary>
        /// Backend method for the search command. This method uses the peer to search the network and loads the results in the 
        /// Response list.
        /// </summary>
        /// <param name="args">Unused params</param>
        private void searchFn(object args=null)
        {
            //MessageBox.Show(Query);
            IList<KademliaResource> res = peer.SearchFor(this.Query);
            responseList.Clear();
            if ((res != null)&&(res.Count>0))
            {
                responseList = new ObservableCollection<KademliaResource>(res);
            }
            else
            {
                MessageBox.Show("Unable to find resource for query \"" + this.Query + "\"", "Search", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            res.Clear();
            //Query = "";
            NotifyPropertyChanged("QueryResponse");
        }
        /// <summary>
        /// Backend method for the start stream command. This method calls the <see cref="OnStreamRequest"/>
        /// </summary>
        /// <param name="args">KademliaResource that has been selected</param>
        private void startStream(object args = null)
        {
            MessageBox.Show(args.GetType().FullName);
            OnStreamRequest(new StreamRequestedArgs(args as KademliaResource));
        }
        #endregion
        /// <summary>
        /// This method raises the stream request event used by the audio player to start a streaming session
        /// </summary>
        /// <param name="args">Parameter that will be passed to the handler</param>
        public virtual void OnStreamRequest(StreamRequestedArgs args)
        {
            StreamRequestedEventHandler handler = StreamRequested;
            if (handler != null)
            {
                handler(this, args);
            }

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
    /// <summary>
    /// Calss containing the information about the Stream Request event 
    /// </summary>
    public class StreamRequestedArgs : EventArgs
    {
        /// <summary>
        /// Resource associated to this event
        /// </summary>
        private KademliaResource res;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="rs"></param>
        public StreamRequestedArgs(KademliaResource rs = null)
        {
            res = rs;
        }
        /// <summary>
        /// Resource associated to this event
        /// </summary>
        public KademliaResource RequestedResource
        {
            get
            {
                return res;
            }
        }
    }
}