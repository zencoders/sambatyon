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
	/// Logica di interazione per LocalStoreWindow.xaml
	/// </summary>
	public partial class LocalStoreWindow : Window
	{
		public LocalStoreWindow()
		{
			this.InitializeComponent();
			
			// Inserire il codice richiesto per la creazione dell'oggetto al di sotto di questo punto.
		}

        private void addFile_Click(object sender, RoutedEventArgs e)
        {
            this.QueryField.IsEnabled = false;
            LocalStoreModel vm = this.DataContext as LocalStoreModel;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".mp3"; // Default file extension
            dlg.Filter = "Mp3 File (.mp3)|*.mp3"; // Filter files by extension
            dlg.Multiselect = true;            

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string[] filenames = dlg.FileNames;
                int successCount = 0;
                List<string> failList = new List<string>();
                foreach (string fl in filenames)
                {
                    bool resp=vm.StoreFile(fl);
                    if (resp)
                    {
                        successCount++;
                    }
                    else
                    {
                        failList.Add(fl);
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(successCount);
                sb.Append((successCount == 1) ? " file has " : " files have ");
                sb.Append("been successfully added to the local music store");
                if (failList.Count > 0)
                {
                    sb.Append("\n");
                    sb.Append(failList.Count);
                    sb.Append((failList.Count == 1) ? " file is " : " files are ");
                    sb.Append(" already contained in the store.\n");
                    foreach (string fl in failList)
                    {
                        sb.Append(" - ");
                        sb.Append(fl);
                        sb.Append("\n");
                    }
                }
                MessageBox.Show(this, sb.ToString(), "Store Files", MessageBoxButton.OK, MessageBoxImage.Information);
                vm.RefreshList.Execute(null);
                vm.RefreshList.Execute(null);
                this.QueryField.IsEnabled = true;
            }
        }
        private void refreshList_Click(object sender, RoutedEventArgs e)
        {
            LocalStoreModel vm = this.DataContext as LocalStoreModel;
            this.QueryField.IsEnabled = false;
            vm.RefreshList.Execute(null);
            this.QueryField.IsEnabled = true;
        }
	}
}