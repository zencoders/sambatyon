using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.IO;
using p2p_player.PeerService;
using p2p_player.PeerPlayer;

namespace p2p_player
{
	public class AudioPlayerModel : INotifyPropertyChanged
	{
        public MediaElement media = new MediaElement();
        public PeerClient peer= new PeerClient();
		public AudioPlayerModel()
		{
            //media.SetSource(s);            
		}
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
	}
}