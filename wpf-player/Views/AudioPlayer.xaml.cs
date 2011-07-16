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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpf_player
{
	/// <summary>
	/// Logica di interazione per AudioPlayer.xaml
	/// </summary>
	public partial class AudioPlayer : UserControl
	{
        private AudioPlayerModel vm= null;
		public AudioPlayer()
		{            
			this.InitializeComponent();            
            this.Dispatcher.ShutdownStarted += new EventHandler(Dispatcher_ShutdownStarted);
			// Inserire il codice richiesto per la creazione dell'oggetto al di sotto di questo punto.
		}
        public void SetDataContext(AudioPlayerModel model)
        {
            vm = model;
            this.DataContext = vm;
        }

        void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

        private void Slider_MouseEnter(object sender, MouseEventArgs e)
        {
            Slider s = sender as Slider;
            Point p=e.GetPosition(s);
            object cItem = this.FindName("SliderCanvas");
            Canvas canvas = cItem as Canvas;
            Point globalP = e.GetPosition(canvas);
            double myTime = (p.X / s.ActualWidth) * ((AudioPlayerModel)this.DataContext).Length;
            object item = this.FindName("position_indicator");
            if ((item != null) && (item is PositionIndicator))
            {
                PositionIndicator indicator = item as PositionIndicator;
                indicator.Visibility = Visibility.Visible;
                indicator.SetValue(Canvas.LeftProperty,p.X-16);
                object item_label = indicator.FindName("TimePositionLabel");
                if ((item_label != null) && (item_label is TextBlock))
                {
                    TextBlock label = item_label as TextBlock;
                    LengthConverter converter = new LengthConverter();
                    label.Text = (string)converter.Convert(myTime, typeof(string), null, null);
                    //Console.WriteLine(myTime);
                }
                //indicator.Margin = new Thickness(globalP.X, 0, 0, 0);
            }
        }

        private void TrackSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            object item = this.FindName("position_indicator");
            if ((item != null) && (item is PositionIndicator))
            {
                PositionIndicator indicator = item as PositionIndicator;
                indicator.Visibility = Visibility.Hidden;
            }
        }

        private void TrackSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            AudioPlayerModel vm = (AudioPlayerModel)this.DataContext;
            vm.EnableFlowRestart = false;
            vm.Pause.Execute(null);            
        }
        private void TrackSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            AudioPlayerModel vm=(AudioPlayerModel)this.DataContext;
            vm.EnableFlowRestart = true;
            Slider s = sender as Slider;
            Console.WriteLine("_________" + s.Value);
            if (!vm.CheckWaitingBuffering((long)s.Value))
            {
                vm.Pause.Execute(null);
            }
        }

        private void TrackSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Slider s = sender as Slider;
            Point p = e.GetPosition(s);
            AudioPlayerModel mv = (AudioPlayerModel)this.DataContext;
            long mySize = (long)((p.X / s.ActualWidth) * mv.ResourceTag.FileSize);
            mv.Position = mySize;
        }
	}
}