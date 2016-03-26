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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Threading;
using System.ServiceModel;
using HangmanLibrary;

namespace HangmanGUIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, IClientCallback
    {
        private delegate void UIUpdateDelegate();

        private IGameState m_gameState;
        private Player m_player;

        public MainWindow(string playerName)
        {
            InitializeComponent();

            try
            {
                // Open a communication channel with the service and retrieve the game state.
                DuplexChannelFactory<IGameState> gameStateFactory =
                    new DuplexChannelFactory<IGameState>(this, "GameState");
                m_gameState = gameStateFactory.CreateChannel();

                // Join the game
                m_player = m_gameState.RegisterPlayer(playerName);

                // If the game is full, exit
                if (m_player == null)
                {
                    MessageBox.Show(
                        "Game is full. Try joining again later.",
                        "Game Full",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    Close();
                }

                Title = m_player.Name + " - HangmanWCF";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Client Callback
        public void UpdateUI()
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                // UI update code here...
                m_player = m_gameState.Players[m_player.PlayerIndex - 1];   // Update the reference
                icLetters.ItemsSource = m_gameState.LettersRemaining;
                icPlayers.ItemsSource = m_gameState.Players;
            }
            else
            {
                // Call asynchronously
                Dispatcher.BeginInvoke(new UIUpdateDelegate(UpdateUI));
            }
        }

        private void Letter_Click(object sender, RoutedEventArgs e)
        {
            if (m_player.HasTurn)
            {
                char letter = (char)((Button)e.Source).Content;
                m_gameState.GuessLetter(letter);
            }
            else
            {
                MessageBox.Show("Wait your turn...");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (m_player != null)
            //    m_gameState.LeaveGame(m_player);
        }
    }
}
