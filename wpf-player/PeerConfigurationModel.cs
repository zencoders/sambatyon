using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace wpf_player
{
    class PeerConfigurationModel:INotifyPropertyChanged
    {
        private int udpPort=9997;
        private int kademliaPort=10000;
        public PeerConfigurationModel(int udpPort, int kademliaPort)
        {
            this.udpPort = udpPort;
            this.kademliaPort = kademliaPort;
        }
        public int UdpPort
        {
            get
            {
                return udpPort;
            }
            set
            {
                this.udpPort = value;
                NotifyPropertyChanged("UdpPort");
            }
        }
        public int KademliaPort
        {
            get
            {
                return kademliaPort;
            }
            set
            {
                this.kademliaPort = value;
                NotifyPropertyChanged("KademliaPort");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
