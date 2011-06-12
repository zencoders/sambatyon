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
using System.IO;
using System.Threading;

namespace wpf_player
{
    public class FakePeer
    {
        private List<KademliaResource> res = new List<KademliaResource>();
        private Dictionary<string, string> fileMap = new Dictionary<string, string>();
        private Thread th=null;
        public int ChunkLength
        {
            get
            {
                return 60;
            }
        }
        public FakePeer()
        {
            KademliaResource tr = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\Garden.mp3", new DhtElement() { Url = new Uri("http://localhost:1292") });
            res.Add(tr);
            fileMap.Add(tr.Id, @"C:\prog\p2p-player\Examples\Resource\Garden.mp3");
            tr = new KademliaResource(@"C:\prog\p2p-player\Examples\Resource\SevenMP3.mp3", new DhtElement() { Url = new Uri("http://localhost:5213") }, new DhtElement() { Url = new Uri("http://localhost:5411") });
            res.Add(tr);
            fileMap.Add(tr.Id, @"C:\prog\p2p-player\Examples\Resource\SevenMP3.mp3");
        }
        public void GetFlow(string RID, int begin, int length, Dictionary<string, float> nodes, Stream s)
        {
            if (th != null) return;
            string filename="";
            if (fileMap.TryGetValue(RID, out filename))
            {
                var param = new
                {
                    path = filename,
                    stream = s
                };
                this.th = new Thread((p)=>
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
        public void StoreFile(string filename)
        {
        }
        public IList<KademliaResource> SearchFor(string queryString)
        {                        
            return new List<KademliaResource>(res);
        }
    }
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private AudioPlayerModel playerModel = null;
        private SearchListModel listModel = null;
        private FakePeer peer = null;
		public MainWindow()
		{
            peer = new FakePeer();
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

		private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.Close();
		}

        private void about_item_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog dlg = new AboutDialog();
            dlg.Owner = this;
            dlg.ShowDialog();
        }
	}
}