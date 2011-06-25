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
using Persistence;
using Persistence.RepositoryImpl;
using System.IO;
using System.Threading;
using System.Linq;
using PeerPlayer;

namespace wpf_player
{    
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private AudioPlayerModel playerModel = null;
        private SearchListModel listModel = null;
        private LocalStoreModel storeModel=null;
        private Peer peer = null;
		public MainWindow()
		{
            bool keepTry = true;
            while (keepTry)
            {
                try
                {
                    peer = new Peer();
                    peer.RunLayers(true);
                    keepTry = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Peer initialization", MessageBoxButton.OK, MessageBoxImage.Error);
                    int udpPort=((peer!=null)?int.Parse(peer.ConfOptions["udpPort"]):0);
                    int kadPort=((peer!=null)?int.Parse(peer.ConfOptions["kadPort"]):0);
                    PeerConfigurationModel vm = new PeerConfigurationModel(udpPort,kadPort);
                    PeerSettingsDialog dlg = new PeerSettingsDialog();
                    dlg.DataContext = vm;
                    dlg.ShowDialog();
                    if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
                    {
                        peer.Configure(vm.UdpPort.ToString(), vm.KademliaPort.ToString());
                        keepTry = true;
                    }
                    else
                    {
                        keepTry = false;
                    }
                }
            }
            if (peer == null) Environment.Exit(0);
            playerModel = new AudioPlayerModel(peer);
            listModel = new SearchListModel(peer);
            listModel.StreamRequested += playerModel.SetResourceHandler;
			this.InitializeComponent();
            object item = this.FindName("StreamPlayer");
            if ((item != null)&&(item is AudioPlayer))
            {
                AudioPlayer p = item as AudioPlayer;
                p.SetDataContext(playerModel);
            }
            item = this.FindName("KadSearchList");
            if ((item != null))
            {
                SearchList s = item as SearchList;
                s.SetDataContext(listModel);
            }            
			// Insert code required on object creation below this point.
		}
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (peer != null)
            {
                peer.Dispose();
            }
            base.OnClosing(e);
        }
		private void exit_item_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.Close();
		}        
        private void peer_config_item_Click(object sender, RoutedEventArgs e)
        {
            PeerConfigurationModel vm = new PeerConfigurationModel(int.Parse(peer.ConfOptions["udpPort"]),int.Parse(peer.ConfOptions["kadPort"]));
            PeerSettingsDialog dlg = new PeerSettingsDialog();
            dlg.DataContext = vm;
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                peer.Configure(vm.UdpPort.ToString(), vm.KademliaPort.ToString());
                MessageBox.Show(this, "Peer Settings successfully changed", "Peer Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void about_item_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void local_store_Click(object sender, RoutedEventArgs e)
        {
            storeModel = new LocalStoreModel(peer);
            LocalStoreWindow dlg = new LocalStoreWindow();
            dlg.DataContext = storeModel;
            dlg.Owner = this;
            dlg.Show();
        }
	}

    public class FakePeer
    {
        private List<KademliaResource> res = new List<KademliaResource>();
        private Dictionary<string, string> fileMap = new Dictionary<string, string>();
        private Thread th = null;
        public Dictionary<string, string> ConfOptions {get; set;}
        public Persistence.Repository tRep;
        public int ChunkLength
        {
            get
            {
                return 60;
            }
        }
        public FakePeer()
        {
            ConfOptions = new Dictionary<string, string>();
            ConfOptions["udpPort"]="9997";
            ConfOptions["kadPort"]="10000";
            KademliaResource tr = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3", new DhtElement() { Url = new Uri("http://localhost:1292") });
            res.Add(tr);
            fileMap.Add(tr.Id, @"C:\prog\p2p-player\Examples\Resource\Garden.mp3");
            tr = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\SevenMP3.mp3", new DhtElement() { Url = new Uri("http://localhost:5213") }, new DhtElement() { Url = new Uri("http://localhost:5411") });
            res.Add(tr);
            fileMap.Add(tr.Id, @"C:\prog\p2p-player\Examples\Resource\SevenMP3.mp3");
            Persistence.RepositoryConfiguration conf = new Persistence.RepositoryConfiguration(new { data_dir = "..\\..\\Resource\\TrackDb" });
            tRep = Persistence.RepositoryFactory.GetRepositoryInstance("Raven", conf);
        }
        public void GetFlow(string RID, int begin, int length, Dictionary<string, float> nodes, Stream s)
        {
            if (th != null) return;
            string filename = "";
            if (fileMap.TryGetValue(RID, out filename))
            {
                var param = new
                {
                    path = filename,
                    stream = s
                };
                this.th = new Thread((p) =>
                {
                    FileStream inFile = null;
                    try
                    {
                        inFile = new FileStream((p as dynamic).path, FileMode.Open);
                        inFile.Seek(begin * 60000, SeekOrigin.Begin);
                        int offset = 0;
                        int buffsize = 60000;
                        byte[] buff = new byte[buffsize];
                        int count = 0;
                        while ((count = inFile.Read(buff, 0, buffsize)) != 0)
                        {
                            try
                            {
                                s.Write(buff, 0, count);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("TH(in) => " + e.Message);
                                return;
                            }
                            //Console.WriteLine(s.Position);
                            offset += count;
                            Thread.Sleep(4000);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("TH(ext) => " + e.Message);
                        return;
                    }
                    finally
                    {
                        inFile.Close();
                    }
                }
                );
                th.Start(param);
            }
        }
        public void StopFlow()
        {
            if (th != null)
            {
                this.th.Interrupt();
                this.th.Join();
                this.th = null;
            }
        }
        public void Configure(string udpPort, string kademliaPort)
        {
        }
        public bool StoreFile(string filename)
        {
            TrackModel track = new TrackModel(filename);
            TrackModel sameTk = new TrackModel();
            this.tRep.GetByKey<TrackModel.Track>(track.GetAsDatabaseType().Id, sameTk);
            if ((sameTk != null) && (sameTk.GetAsDatabaseType().Id == track.GetAsDatabaseType().Id))
            {
                return false;
            }
            else
            {
                this.tRep.Save(track);
                return true;
            }
        }
        public IList<KademliaResource> SearchFor(string queryString)
        {
            return new List<KademliaResource>(res);
        }
        public IList<TrackModel.Track> GetAllTracks()
        {
            List<TrackModel.Track> list = new List<TrackModel.Track>();
            tRep.GetAll(list);
            return list;           
        }
    }
}