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
using System.IO;
using p2p_player.PeerService;

namespace p2p_player
{
	/// <summary>
	/// Logica di interazione per AudioPlayer.xaml
	/// </summary>
	public partial class AudioPlayer : UserControl
	{
		AudioPlayerModel m= new AudioPlayerModel();
		public AudioPlayer()
		{
			this.InitializeComponent();
            DataContext = m;
			// Inserire il codice richiesto per la creazione dell'oggetto al di sotto di questo punto.
		}
	}
}