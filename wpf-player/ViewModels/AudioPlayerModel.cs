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
using NAudio.FileFormats.Mp3;
using PeerPlayer;


namespace wpf_player
{
	public class AudioPlayerModel : INotifyPropertyChanged,IDisposable
	{
        private Peer peer = null;
        private ObservableStream localstream=null;
        private MemoryStream lmem = null;
        private byte[] streambuff = null;
        private KademliaResource rsc=null;
        private bool bufferingState=false;
        WaveChannel32 wc=null;
        private IWavePlayer player = new WaveOut();
        private long pos = 0;
        private double currentTime=0.0;
        private long startPosition = 0L;
        private bool startPhaseBuffering = false;
        public AudioPlayerModel(Peer p = null)
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
        public bool BufferingState
        {
            get
            {
                return bufferingState;
            }
            private set
            {
                bufferingState = value;
                NotifyPropertyChanged("BufferingState");
            }
        }
        public bool HasResource
        {
            get
            {
                return rsc != null;
            }
        }
        public string PlayingState
        {
            get
            {
                if (BufferingState)
                {
                    return "Buffering ...";
                } else {
                    PlaybackState st = player.PlaybackState;
                    switch (st)
                    {
                        case PlaybackState.Playing : 
                            return "Playing";
                        case PlaybackState.Stopped:
                            return "Stopped";
                        case PlaybackState.Paused:
                            return "Paused";
                        default:
                            return "";
                    }
                }
            }
        }
        public KeyValuePair<long,long> BufferPortion
        {
            get
            {
                if (localstream != null)
                {
                    return new KeyValuePair<long,long>(startPosition,localstream.Position);
                }
                else
                {
                    return new KeyValuePair<long, long>(startPosition, 0);
                }
            }            
        }
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
        public bool EnableFlowRestart
        {
            get;
            set;
        }
        public long Position
        {
            get
            {                
                return pos;
                
            }
            set
            {
                if (this.wc != null)
                {
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
            //NotifyPropertyChanged("Position");                                
            //localstream = new FileStream(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3",FileMode.Open);                
            //if (localstream == null) return;      
            //long lastpos = (lmem!=null?lmem.Position:startPosition);
            long lastpos = (lmem != null ? Position : startPosition);
            lmem = new MemoryStream(streambuff);
            resetWaveStream();
            if (player.PlaybackState != PlaybackState.Paused)
            {
                //rsc = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3");
                //NotifyPropertyChanged("ResourceTag");
                Position = startPosition;                
            }
            else
            {
                Position = lastpos;
            }
            player.Init(wc);            
            player.Play();
            NotifyPropertyChanged("PlayingState");
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
                /*player.Init(wc);
                player.Play();*/
                this.play();
            }
            else if (startPhaseBuffering)
            {
                startPhaseBuffering = false;
                this.play();
            }
            NotifyPropertyChanged("PlayingState");
        }
        private void stop(object args=null)
        {            
            if (player.PlaybackState != PlaybackState.Stopped)
            {
                player.Stop();
                Position = 0;
                CurrentTime = 0;
                BufferingState = false;
            }
            if (wc!=null)
            {
                //MessageBox.Show(wc.Position.ToString());
                wc.Close();
                wc = null;
            }
            NotifyPropertyChanged("PlayingState");
            peer.StopFlow();
        }
        #endregion
        #region Callback
        public void playback_stopped(object sender, EventArgs args)
        {
            MessageBox.Show(args.ToString());            
            this.stop();
        }
        private void wc_Sample(object sender, SampleEventArgs e)
        {
            if (wc == null) return;
            if (wc.CurrentTime.TotalSeconds != currentTime)
            {
                currentTime = wc.CurrentTime.TotalSeconds;
                NotifyPropertyChanged("CurrentTime");
                NotifyPropertyChanged("BufferPortion");
                pos = lmem.Position;
                NotifyPropertyChanged("Position");
            }
            if (lmem.Position == lmem.Length) { this.stop(); return; }
            long myBufferSize = 20480;
            if ((!BufferingState) && (localstream.Position != rsc.Tag.FileSize) && ((localstream.Position - lmem.Position) < myBufferSize))
            {
                Console.WriteLine("Paused ... waiting!");
                BufferingState = true;
                this.pause();
                localstream.WaitForMore();
            }
        }
        private void resumePlay(object sender, EventArgs args)
        {
            BufferingState = false;
            Console.WriteLine("Resuming !");
            Position = pos;
            this.pause();            
        }
        public void SetResourceHandler(object sender, StreamRequestedArgs args)
        {
            setResource(args.RequestedResource);
        }
        #endregion
        public bool CheckWaitingBuffering(long newPosition)
        {
            if ((localstream != null) && (newPosition > localstream.Position))
            {
                Console.WriteLine("Too far ... waiting!");
                BufferingState = true;
                //this.pause();
                localstream.WaitForMore(Math.Max(0, (int)(newPosition - localstream.Position)+20480));
                return true;
            } else
            {
                return false;
            }
        }
        private void setResource(KademliaResource rsc)
        {
            this.stop();            
            if (rsc == null) return;
            if ((this.rsc!=null)&&(rsc.Id.Equals(this.rsc.Id))) return;
            this.rsc = rsc;
            setupLocalStream(this.rsc, 0);              
        }
        private void setupLocalStream(KademliaResource rsc,long spos)
        {
            if (localstream != null)
            {
                localstream.Close();
            }
            EnableFlowRestart = true;
            Dictionary<string, float> tD = new Dictionary<string, float>();
            foreach (DhtElement de in rsc.Urls)
            {
                tD.Add(de.Url.ToString(), 0);
            }
            this.streambuff = new byte[rsc.Tag.FileSize];
            localstream = new ObservableStream(streambuff);
            startPosition = spos;
            localstream.Seek(spos, SeekOrigin.Begin);
            pos = spos;
            NotifyPropertyChanged("Position");
            NotifyPropertyChanged("BufferPortion");
            localstream.WaitedPositionReached += resumePlay;
            localstream.PositionChanged += (sender, args) => { NotifyPropertyChanged("BufferPortion"); };
            peer.GetFlow(rsc.Id,convertSizeToChunk(startPosition), convertSizeToChunk(rsc.Tag.FileSize), tD, localstream);
            BufferingState = true;
            NotifyPropertyChanged("PlayingState");
            startPhaseBuffering = true;
            localstream.WaitForMore();            
            NotifyPropertyChanged("ResourceTag");
            NotifyPropertyChanged("Length");
            NotifyPropertyChanged("BigBufferSize");
            NotifyPropertyChanged("HasResource");
        }
        private void resetWaveStream()
        {            
            Mp3FileReader mp3file = new Mp3FileReader(lmem);                        
            wc = new WaveChannel32(mp3file);            
            Console.WriteLine(mp3file.WaveFormat);
            wc.Sample += new EventHandler<SampleEventArgs>(wc_Sample);
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
        private int convertSizeToChunk(long size)
        {
            int cl = this.peer.ChunkLength * 1024;
            if (cl == 0) return 0;
            return (int)(size / cl);
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

        #region IDisposable

        public void Dispose()
        {
            this.close();
        }

        #endregion
    }    
}