using System;
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
