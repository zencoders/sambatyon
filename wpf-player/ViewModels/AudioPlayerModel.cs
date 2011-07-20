/*****************************************************************************************
 * 
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
using PeerLibrary;


namespace wpf_player
{
    /// <summary>
    /// Class that implements the ViewModel in MVVM pattern. 
    /// This class contains the logic for playing audio stream using the Peer as
    /// point of access for the network. 
    /// </summary>
	public class AudioPlayerModel : INotifyPropertyChanged,IDisposable
	{
        /// <summary>
        /// Local Reference to the Peer
        /// </summary>
        private Peer peer = null;
        /// <summary>
        /// Observable stream where the Peer writes data collected from the network
        /// </summary>
        private ObservableStream localstream=null;
        /// <summary>
        /// Stream used by the player to read the byte buffer.
        /// </summary>
        private MemoryStream lmem = null;
        /// <summary>
        /// Byte buffer pointed by both <c>localstream</c> and <c>lmem</c> containing the raw data.
        /// </summary>
        private byte[] streambuff = null;
        /// <summary>
        /// The resource that the player is currently handling
        /// </summary>
        private KademliaResource rsc=null;
        /// <summary>
        /// Flag that indicates if the player is in buffering state
        /// </summary>
        private bool bufferingState=false;
        /// <summary>
        /// NAudio channel used to read the stream
        /// </summary>
        WaveChannel32 wc=null;
        /// <summary>
        /// NAudio player used to read wave channel.
        /// </summary>
        private IWavePlayer player = new WaveOut();
        /// <summary>
        /// Current position inside the byte buffer
        /// </summary>
        private long pos = 0;
        /// <summary>
        /// Current position in time 
        /// </summary>
        private double currentTime=0.0;
        /// <summary>
        /// Start position
        /// </summary>
        private long startPosition = 0L;
        /// <summary>
        /// Flag used to identify if the player is in the buffering state that happens at the begining 
        /// of the streaming session
        /// </summary>
        private bool startPhaseBuffering = false;
        /// <summary>
        /// Constructor that initializes all commands and the reference to the Peer.
        /// </summary>
        /// <param name="p"></param>
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
        /// <summary>
        /// Flag used to check if the player is in buffering state
        /// </summary>
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
        /// <summary>
        /// Flag used to check if the player is handling a resource
        /// </summary>
        public bool HasResource
        {
            get
            {
                return rsc != null;
            }
        }
        /// <summary>
        /// Property containing a string representation of the playing state of the player
        /// </summary>
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
        /// <summary>
        /// Property that contains the portion of Buffer currently used.
        /// IMPORTANT: by now the start position of the buffer portion is always 0
        /// </summary>
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
        /// <summary>
        /// Tag information of the currently handling resource
        /// </summary>
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
        /// <summary>
        /// Current volume value
        /// </summary>
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
        /// <summary>
        /// Max volume value
        /// </summary>
        public float MaxVolume
        {
            get;
            private set;
        }
        /// <summary>
        /// Flag that indicates if flow restart is enabled
        /// </summary>
        public bool EnableFlowRestart
        {
            get;
            set;
        }
        /// <summary>
        /// Current Position of the player inside the byte buffer
        /// </summary>
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
        /// <summary>
        /// Current position expressed in seconds from the start
        /// </summary>
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
        /// <summary>
        /// Size of the shared byte buffer
        /// </summary>
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
        /// <summary>
        /// Lenght of the resource. This information is obtained from the tag.
        /// </summary>
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
        /// <summary>
        /// Play Command
        /// </summary>
        public ICommand Play
        {
            get;
            private set;
        }
        /// <summary>
        /// Close command
        /// </summary>
        public ICommand Close
        {
            get;
            private set;
        }
        /// <summary>
        /// Stop command
        /// </summary>
        public ICommand Stop
        {
            get;
            private set;
        }
        /// <summary>
        /// Pause Command
        /// </summary>
        public ICommand Pause
        {
            get;
            private set;
        }
        #endregion
        #region Command Backfunction
        /// <summary>
        /// Method used to start (or restart) the execution of the track. This resets peer stream and wave stream then 
        /// initializes the NAudio player and calls its <c>Play</c> method
        /// </summary>
        /// <param name="args">Unused params</param>
        /// <seealso cref="resetWaveStream"/>
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
        /// <summary>
        /// This method closes all streams and the disposes the player.
        /// </summary>
        /// <param name="args">Unused Params</param>
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
        /// <summary>
        /// This method handle the pause operation. If the player is currently playing the methods calls NAudio player <c>Pause</c>
        /// method, if the player is paused then the <see cref="play"/> method will called, if the player is in the buffering phase
        /// at the start of the streaming session the method try to close the buffer phase and start the playing phase.
        /// </summary>
        /// <param name="args">Unused params</param>
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
        /// <summary>
        /// This method handles the stop operation. If the player is not stopped, the NAudio player will be stopped and some
        /// structure will be cleaned
        /// </summary>
        /// <param name="args">Unused Params</param>
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
        /// <summary>
        /// Unused callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void playback_stopped(object sender, EventArgs args)
        {
            MessageBox.Show(args.ToString());            
            this.stop();
        }
        /// <summary>
        /// This callback is called everytime a sample is player. This method handles current time and current position updates,
        /// the stop of the playing at the end of the track and the buffering state.
        /// </summary>
        /// <param name="sender">Unused params</param>
        /// <param name="e">Unused params</param>
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
        /// <summary>
        /// This callback handles playback resuming.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void resumePlay(object sender, EventArgs args)
        {
            BufferingState = false;
            Console.WriteLine("Resuming !");
            Position = pos;
            this.pause();            
        }
        /// <summary>
        /// Callback used when the streaming is requested using the seach list. This method calls the <see cref="setResource"/> method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SetResourceHandler(object sender, StreamRequestedArgs args)
        {
            setResource(args.RequestedResource);
        }
        #endregion
        /// <summary>
        /// This method check if the new position is beyond the limit of buffered data; in this case the playback is paused and
        /// the player will wait for missing data.
        /// </summary>
        /// <param name="newPosition">Position that we want to check</param>
        /// <returns>True if the position is beyond and the player has to wait, false otherwise</returns>
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
        /// <summary>
        /// Sets the current handling resource. If the player is handling the same resource then the methods call <see cref="restartStream"/>
        /// otherwise it calls <see cref="setupLocalStream"/>
        /// </summary>
        /// <param name="rsc">Resource that has to be handled</param>
        private void setResource(KademliaResource rsc)
        {
            this.stop();            
            if (rsc == null) return;
            if ((this.rsc != null) && (rsc.Id.Equals(this.rsc.Id)))
            {
                restartStream();
            }
            else
            {
                this.rsc = rsc;
                setupLocalStream(this.rsc, 0);
            }
        }
        /// <summary>
        /// This method resets all structure of the player related to the playback operation but does not modify the peer state (this
        /// way the peer keep downloading the file).
        /// </summary>
        private void restartStream()
        {
            NotifyPropertyChanged("Position");
            NotifyPropertyChanged("BufferPortion");
            localstream.WaitedPositionReached += resumePlay;
            localstream.PositionChanged += (sender, args) => { NotifyPropertyChanged("BufferPortion"); };
            peer.RestartFlow();
            BufferingState = true;
            NotifyPropertyChanged("PlayingState");
            startPhaseBuffering = true;
            localstream.WaitForMore();
            NotifyPropertyChanged("ResourceTag");
            NotifyPropertyChanged("Length");
            NotifyPropertyChanged("BigBufferSize");
            NotifyPropertyChanged("HasResource");
        }
        /// <summary>
        /// Sets all configuration to enable the playback and the downloading of the track from the network.
        /// After setting all configuration, the player will wait for the buffer to be full enough to start playing.
        /// </summary>
        /// <param name="rsc">Resource to Handle</param>
        /// <param name="spos">Starting position</param>
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
                Console.WriteLine(de.Url);
                tD[de.Url.ToString()] = 0;
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
            peer.GetFlow(rsc.Tag.FileHash,(int)startPosition, (int)rsc.Tag.FileSize, tD, localstream);
            BufferingState = true;
            NotifyPropertyChanged("PlayingState");
            startPhaseBuffering = true;
            localstream.WaitForMore(60000);            
            NotifyPropertyChanged("ResourceTag");
            NotifyPropertyChanged("Length");
            NotifyPropertyChanged("BigBufferSize");
            NotifyPropertyChanged("HasResource");
        }
        /// <summary>
        /// Resets playback related streams and channel.
        /// </summary>
        private void resetWaveStream()
        {            
            Mp3FileReader mp3file = new Mp3FileReader(lmem);                        
            wc = new WaveChannel32(mp3file);            
            Console.WriteLine(mp3file.WaveFormat);
            wc.Sample += new EventHandler<SampleEventArgs>(wc_Sample);
        }
        /// <summary>
        /// Converts a position in the buffer to a time instant withing the range of the song lenght
        /// </summary>
        /// <param name="p">Position that we want to convert</param>
        /// <returns>The time position in seconds</returns>
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
        /// <summary>
        /// Convert a size in byte to a number of chunk.
        /// </summary>
        /// <param name="size">Size to convert</param>
        /// <returns>Number of chunk corresponding to the given size. The number is rounded by excess </returns>
        private int convertSizeToChunk(long size)
        {
            int cl = this.peer.ChunkLength * 1024;
            if (cl == 0) return 0;
            return (int)(size / cl);
        }
		#region INotifyPropertyChanged
        /// <summary>
        /// Event for the property changed (this is used by WPF Data Binding)
        /// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method called when a property has been changed. This method rises the event PropertyChange.
        /// </summary>
        /// <param name="info">Name of the property that has been changed</param>
		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

        #region IDisposable

        /// <summary>
        /// Disposes the player calling the <see cref="close"/> method
        /// </summary>
        public void Dispose()
        {
            this.close();
        }

        #endregion
    }    
}