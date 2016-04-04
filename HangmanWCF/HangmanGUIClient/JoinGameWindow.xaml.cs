using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HangmanGUIClient
{
    /// <summary>
    /// Interaction logic for JoinGameWindow.xaml
    /// </summary>
    public partial class JoinGameWindow : Window
    {
        public JoinGameWindow()
        {
            InitializeComponent();
            tbName.Focus();
        }

        private void JoinGame_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbName.Text))
            {
                MainWindow mainWindow = new MainWindow(tbName.Text);
                mainWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Player name cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void on_KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(tbName.Text))
                {
                    MainWindow mainWindow = new MainWindow(tbName.Text);
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Player name cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
