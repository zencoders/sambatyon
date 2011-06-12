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
	/// Logica di interazione per PeerSettingsDialog.xaml
	/// </summary>
	public partial class PeerSettingsDialog : Window
	{
		public PeerSettingsDialog()
		{
			this.InitializeComponent();
			
			// Inserire il codice richiesto per la creazione dell'oggetto al di sotto di questo punto.
		}
        private bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }
            }

            // All dependency objects are valid
            return true;
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid(this))
            {
                MessageBox.Show(this, "Some port number are not valid", "Peer Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                this.DialogResult = true;
                this.Close();
            }
        }      
	}
}