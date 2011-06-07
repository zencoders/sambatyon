using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.IO;
using System.Windows;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Windows.Input;
using Persistence;
using System.Windows.Markup;
using Persistence.Tag;


namespace wpf_player
{
	public class AudioPlayerModel : INotifyPropertyChanged,IDisposable
	{
        private FakePeer peer = null;
        private ObservableStream localstream=null;
        private MemoryStream lmem = null;
        private byte[] streambuff = null;
        private KademliaResource rsc=null;
        private bool bufferingState=false;
        //private Stream localstream = null;
        //private PlayerEx player=new PlayerEx();
        WaveChannel32 wc=null;
        private IWavePlayer player = new WaveOut();
        //private Mp3Reader wr = null;
        private long pos = 0;
        private double currentTime=0.0;
        //private int seconds = 3;
        //private int len = 0;
        //private AlwaysExecuteCommand playCmd;
        public AudioPlayerModel(FakePeer p = null)
		{
            //player.Done += new PlayerEx.DoneEventHandler(player_Done);
           //player.SetVolume(200, 200);
            player.PlaybackStopped += new EventHandler(playback_stopped);
            this.peer = p;
            Play = new AlwaysExecuteCommand(play);
            Pause = new AlwaysExecuteCommand(pause);
            Stop = new AlwaysExecuteCommand(stop);
            Close = new AlwaysExecuteCommand(close);
            MaxVolume = 1.0F;
            Volume = 0.4F;
		}
        #region Properties
        public CompleteTag ResourceTag
        {
            get
            {
                if (rsc != null)
                {
                    return rsc.Tag;
                }
                else
                {
                    return new CompleteTag();
                }
            }
        }
        public float Volume
        {
            get
            {
                if (player != null)
                {
                    return player.Volume;
                }
                else
                {
                    return 0.0F;
                }
            }
            set
            {
                if (player != null)
                {
                    player.Volume = value;
                    NotifyPropertyChanged("Volume");
                }
            }
        }
        public float MaxVolume
        {
            get;
            private set;
        }
        public long Position
        {
            get
            {
                /*if (this.wc != null)
                {
                    return this.wc.Position;
                }
                else
                {
                    return 0;
                }*/
                Console.WriteLine("POS:"+pos);
                return pos;
                
            }
            set
            {
                if (this.wc != null)
                {
                    //    double loadedPerc = (double)localstream.Position / localstream.Length;
                    //    double askedPerc = value / this.rsc.Tag.Length;
                    //    if (askedPerc > loadedPerc)
                    //    {
                    //        Console.WriteLine("Wait!!! "+askedPerc+">"+loadedPerc);
                    //    }
                    this.wc.CurrentTime = TimeSpan.FromSeconds(timeFromPosition(value));
                    //    Console.WriteLine(this.wc.CurrentTime);
                }
                pos = value;
                NotifyPropertyChanged("Position");
            }
        }
        public double CurrentTime
        {
            get
            {
                if (wc != null)
                {
                    return wc.CurrentTime.TotalSeconds;
                }
                else
                {

                    return 0;
                }
            }
            set
            {
                currentTime = value;
                NotifyPropertyChanged("CurrentTime");
            }
        }
        public long BigBufferSize
        {
            get
            {
                if (rsc!=null)
                {
                    Console.WriteLine("FS:"+rsc.Tag.FileSize);
                    return rsc.Tag.FileSize;
                }
                else
                {
                    return 1;
                }
            }
        }
        public double Length
        {
            get
            {
                if (this.rsc != null)
                {
                    return rsc.Tag.Length;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion
        #region Command
        public ICommand Play
        {
            get;
            private set;
        }
        public ICommand Close
        {
            get;
            private set;
        }
        public ICommand Stop
        {
            get;
            private set;
        }
        public ICommand Pause
        {
            get;
            private set;
        }
        #endregion
        #region Command Backfunction
        private void play(object args=null)
        {
            if (rsc == null) return;
            if (player.PlaybackState == PlaybackState.Playing) return;            
            if (player.PlaybackState != PlaybackState.Paused)            
            {
                //rsc = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3");
                //NotifyPropertyChanged("ResourceTag");
                Position = 0;
                //NotifyPropertyChanged("Position");                                
                //localstream = new FileStream(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3",FileMode.Open);                
                //if (localstream == null) return;      
                lmem = new MemoryStream(streambuff);
                Mp3FileReader mp3file = new Mp3FileReader(lmem);                
                wc= new WaveChannel32(mp3file);
                wc.Sample += new EventHandler<SampleEventArgs>(wc_Sample);

            }
            player.Init(wc);            
            player.Play();                              
        }        
        private void wc_Sample(object sender, SampleEventArgs e)
        {
            if (wc == null) return;
            if (wc.CurrentTime.TotalSeconds != currentTime)
            {
                currentTime = wc.CurrentTime.TotalSeconds;
                NotifyPropertyChanged("CurrentTime");
                pos = lmem.Position;
                NotifyPropertyChanged("Position");
            }
            if (lmem.Position == lmem.Length) { this.stop(); return; }
            long myBufferSize= 20480;           
            if ((!bufferingState)&&(localstream.Position!=rsc.Tag.FileSize)&&((localstream.Position-lmem.Position)<myBufferSize))
            {
                Console.WriteLine("Paused ... waiting!");
                bufferingState = true;
                this.pause();
                localstream.WaitForMore();
            }
        }
        private void close(object args=null)
        {
            this.stop();
            if (wc != null)
            {
                wc.Close();
                wc = null;
            }
            if (localstream != null)
            {
                localstream.Close();
                localstream = null;
            }
            if (player != null)
            {
                player.Dispose();
            }
        }
        private void pause(object args = null)
        {
            if (player.PlaybackState == PlaybackState.Playing)
            {
                player.Pause();
            }
            else if (player.PlaybackState == PlaybackState.Paused)
            {
                //player.Init(wc);
                player.Play();
            }
        }
        private void stop(object args=null)
        {            
            if (player.PlaybackState != PlaybackState.Stopped)
            {
                player.Stop();
                Position = 0;
                CurrentTime = 0;
                bufferingState = false;
            }
            if (wc!=null)
            {
                //MessageBox.Show(wc.Position.ToString());
                wc.Close();
                wc = null;
            }            
        }
        #endregion
        public void playback_stopped(object sender, EventArgs args)
        {
            MessageBox.Show(args.ToString());
            this.stop();
        }
        public void SetResourceHandler(object sender, StreamRequestedArgs args)
        {
            setResource(args.RequestedResource);
        }
        private void resumePlay(object sender, EventArgs args)
        {
            bufferingState = false;
            Console.WriteLine("Resuming !");
            this.pause();
        }
        private void setResource(KademliaResource rsc)
        {
            this.stop();
            if (rsc == null) return;
            if ((this.rsc!=null)&&(rsc.Id.Equals(this.rsc.Id))) return;
            if (localstream != null)
            {
                localstream.Close();                
            }
            Dictionary<string,float> tD=new Dictionary<string,float>();
            foreach (DhtElement de in rsc.Urls)
            {
                tD.Add(de.Url.ToString(),0);
            }            
            this.rsc = rsc;
            NotifyPropertyChanged("ResourceTag");
            NotifyPropertyChanged("Length");
            NotifyPropertyChanged("BigBufferSize");
            this.streambuff = new byte[rsc.Tag.FileSize];
            localstream = new ObservableStream(streambuff);
            localstream.WaitedPositionReached += resumePlay;
            peer.GetFlow(rsc.Id, 0, 0, tD, localstream);            
        }
        private double timeFromPosition(long p)
        {
            if (rsc != null)
            {
                return ((double)p / rsc.Tag.FileSize) * rsc.Tag.Length;
            }
            else
            {
                return 0.0;
            }
        }
        /*private void ReadData()
        {
            if (pos <= len)
                {
                byte[] data = wr.ReadData(pos, seconds);
                pos += seconds;
                player.AddData(data);
            }
        }
        private void player_Done(object sender, DoneEventArgs args)
        {
            ReadData();
        }*/
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

        #region IDisposable

        public void Dispose()
        {
            this.close();
        }

        #endregion
    }
}