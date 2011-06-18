using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Persistence;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PeerPlayer;

namespace wpf_player
{
	public class SearchListModel : INotifyPropertyChanged
	{
        private ObservableCollection<KademliaResource> responseList=new ObservableCollection<KademliaResource>();
        public delegate void StreamRequestedEventHandler(object sender, StreamRequestedArgs args);
        public event StreamRequestedEventHandler StreamRequested;
        private Peer peer = null;
        private string queryStr;
		public SearchListModel(Peer peer=null)
		{
            this.peer = peer;
            Query = "";            
            Search = new AlwaysExecuteCommand(searchFn);
            RaiseStartStream = new AlwaysExecuteCommand(startStream);            
        }
        #region Property
        public IList<KademliaResource> QueryResponse
        {
            get
            {
                return responseList;
            }
        }
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
        public ICommand RaiseStartStream
        {
            get;
            private set;
        }
        public ICommand Search
        {
            get;
            private set;
        }
        #endregion
        #region Command Backfunction
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
            Query = "";
            NotifyPropertyChanged("QueryResponse");
        }
        private void startStream(object args = null)
        {
            MessageBox.Show(args.GetType().FullName);
            OnStreamRequest(new StreamRequestedArgs(args as KademliaResource));
        }
        #endregion
        public virtual void OnStreamRequest(StreamRequestedArgs args)
        {
            StreamRequestedEventHandler handler = StreamRequested;
            if (handler != null)
            {
                handler(this, args);
            }

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
    public class StreamRequestedArgs : EventArgs
    {
        private KademliaResource res;
        public StreamRequestedArgs(KademliaResource rs = null)
        {
            res = rs;
        }
        public KademliaResource RequestedResource
        {
            get
            {
                return res;
            }
        }
    }
}