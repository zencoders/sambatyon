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


namespace wpf_player
{
    public class LengthConverter : MarkupExtension,IValueConverter
    {
        public LengthConverter() { }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double ll = (double)value;
            TimeSpan ts = TimeSpan.FromSeconds(ll);
            StringBuilder sb=new StringBuilder();
            sb.Append(((long)ts.TotalMinutes).ToString().PadLeft(2,'0'));
            sb.Append(":");
            sb.Append(ts.Seconds.ToString().PadLeft(2,'0'));
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            string[] splitted = str.Split(':');
            if (splitted.Length==2)
            {
                TimeSpan ts = TimeSpan.FromMinutes(Double.Parse(splitted[0]));
                TimeSpan ts_sec=TimeSpan.FromSeconds(Double.Parse(splitted[1]));
                ts.Add(ts_sec);
                return (int)ts.TotalSeconds;
            } else
            {
                return 0;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

	public class AudioPlayerModel : INotifyPropertyChanged,IDisposable
	{
        private Stream localstream;
        private KademliaResource rsc;
        //private Stream localstream = null;
        //private PlayerEx player=new PlayerEx();
        WaveChannel32 wc=null;
        private IWavePlayer player = new WaveOut();
        //private Mp3Reader wr = null;
        private double pos = 0;
        private int seconds = 3;
        private int len = 0;
        private AlwaysExecuteCommand playCmd;
		public AudioPlayerModel()
		{
            //player.Done += new PlayerEx.DoneEventHandler(player_Done);
           //player.SetVolume(200, 200);
            player.PlaybackStopped += new EventHandler(playback_stopped);
            Play = new AlwaysExecuteCommand(play);
            Pause = new AlwaysExecuteCommand(pause);
            Stop = new AlwaysExecuteCommand(stop);
            Close = new AlwaysExecuteCommand(close);
            MaxVolume = 1.0F;
            Volume = 0.4F;
            Prova = "PPPPPPPPPPPPPPP";
		}
        #region Properties
        public string Prova
        {
            get;
            private set;
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
        public double Position
        {
            get
            {
                /*if (this.wc != null)
                {
                    return this.wc.CurrentTime.TotalSeconds;
                }
                else
                {
                    return 0;
                }*/
                return pos;
            }
            set
            {
                if (this.wc != null)
                {
                    this.wc.CurrentTime = TimeSpan.FromSeconds(value);                                        
                }
                pos = value;
                NotifyPropertyChanged("Position");
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
                    return 0.5;
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
            if (player.PlaybackState == PlaybackState.Playing) return;
            if (player.PlaybackState != PlaybackState.Paused)            
            {
                rsc = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3");
                NotifyPropertyChanged("Position");
                NotifyPropertyChanged("Length");
                localstream = new FileStream(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3",FileMode.Open);                
                if (localstream == null) return;
                pos = 0;
                Mp3FileReader mp3file = new Mp3FileReader(localstream);
                wc= new WaveChannel32(mp3file);
                wc.Sample += new EventHandler<SampleEventArgs>(wc_Sample);

            }
            player.Init(wc);            
            player.Play();                              
        }
        void wc_Sample(object sender, SampleEventArgs e)
        {
            if (wc.CurrentTime.TotalSeconds != pos)
            {
                pos = wc.CurrentTime.TotalSeconds;
                NotifyPropertyChanged("Position");
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
                player.Init(wc);
                player.Play();
            }
        }
        private void stop(object args=null)
        {            
            if (player.PlaybackState != PlaybackState.Stopped)
            {
                player.Stop();
                Position = 0;
            }
            if (wc!=null)
            {
                MessageBox.Show(wc.Position.ToString());
                wc.Close();
                wc = null;
            }
            if (localstream != null)
            {
                localstream.Close();
                localstream = null;
            }
        }
        #endregion
        public void playback_stopped(object sender, EventArgs args)
        {
            MessageBox.Show(args.ToString());
            this.stop();
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