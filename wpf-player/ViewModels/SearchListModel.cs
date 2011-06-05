using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Persistence;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace wpf_player
{
	public class SearchListModel : INotifyPropertyChanged
	{
        private ObservableCollection<KademliaResource> responseList=new ObservableCollection<KademliaResource>();
        private string queryStr;
		public SearchListModel()
		{
            Query = "";
            Search = new AlwaysExecuteCommand(searchFn);
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
        public ICommand Search
        {
            get;
            private set;
        }
        #endregion
        #region Command Backfunction
        private void searchFn(object args=null)
        {
            MessageBox.Show(Query);
            List<KademliaResource> res= new List<KademliaResource>();
            res.Add(new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3",new DhtElement() { Url = new Uri("http://localhost:1292") }));
            res.Add(new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\SevenMP3.mp3",new DhtElement() { Url = new Uri("http://localhost:5213") },new DhtElement() { Url = new Uri("http://localhost:5411") }));
            responseList.Clear();
            responseList = new ObservableCollection<KademliaResource>(res);
            res.Clear();
            Query = "";
            NotifyPropertyChanged("QueryResponse");
        }
        #endregion
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