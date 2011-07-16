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