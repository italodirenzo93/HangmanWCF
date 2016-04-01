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

using HangmanLibrary;

namespace HangmanGUIClient
{
    /// <summary>
    /// Interaction logic for GuessWordWindow.xaml
    /// </summary>
    public partial class GuessWordWindow : Window
    {
        private readonly IGameState m_gameState;

        public GuessWordWindow(IGameState gameState)
        {
            InitializeComponent();
            m_gameState = gameState;
        }

        private void btnGuess_Click(object sender, RoutedEventArgs e)
        {
            string word = tbWord.Text.ToUpper();
            if (!string.IsNullOrWhiteSpace(word))
            {
                m_gameState.GuessWord(word);
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Please enter a word.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
