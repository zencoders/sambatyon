using System;
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