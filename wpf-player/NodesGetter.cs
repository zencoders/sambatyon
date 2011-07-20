using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Net;


namespace wpf_player
{
    [RunInstaller(true)]
    public partial class NodesGetter : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NodesGetter()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Method called during installation. This method downloads the nodes file.
        /// </summary>
        /// <param name="stateSaver"></param>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile("http://localhost:8000/nodes.xml", Context.Parameters["targetdir"] + "\\Resource\\nodes.xml");
            }
            catch (Exception e)
            {
            }
        }
    }
}
