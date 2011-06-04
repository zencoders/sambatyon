using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.IO;
//using p2p_player.PeerPlayer;
using System.Windows;
using System.Net.Sockets;

namespace p2p_player
{
	public class AudioPlayerModel : INotifyPropertyChanged,IDisposable
	{
        public MediaElement media = new MediaElement();
        //public PeerClient peer= new PeerClient();
        public Socket sk= new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		public AudioPlayerModel()
		{            
            //peer.ConnectToStreamAsync();
            //peer.CloseAsync();
            //media.SetSource(s);            
		}    
        /*private void connectToStream_completed(object obj, ConnectToStreamCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                MessageBox.Show(args.Error.Message, "Communication Error", MessageBoxButton.OK);
                return;
            }
            MessageBox.Show(args.Result.Length.ToString());
        }*/
        public void Stop()
        {
            media.Stop();
        }
        public void Play()
        {
            media.Play();
        }
        public void Pause()
        {
            media.Pause();
        }
        public void SetResource(Stream s)
        {
            media.SetSource(s);
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

        #region IDisposable Membri di

        public void Dispose()
        {
            //peer.CloseAsync();
        }

        #endregion
    }
}