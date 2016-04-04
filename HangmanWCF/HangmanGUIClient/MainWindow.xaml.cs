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

        private readonly IGameState m_gameState;
        private readonly int m_playerID;
        private bool gameHasEnded = false;

        #region Constructor
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
                m_playerID = m_gameState.RegisterPlayer(playerName);

                // If the game is full, exit
                if (m_playerID < 0)
                {
                    MessageBox.Show(
                        "Game is full. Try joining again later.",
                        "Game Full",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    Close();
                }

                Title = playerName + " - HangmanWCF";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        // Client Callback
        public void UpdateUI()
        {

            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                // UI update code here...
                icLetters.ItemsSource = m_gameState.LettersRemaining;
                icPlayers.ItemsSource = m_gameState.Players;
                icWordInPlay.ItemsSource = m_gameState.LetterTiles;
                tbStatus.Text = m_gameState.UpdateMessage;
                tbWordsRemaining.Text = m_gameState.WordsRemaining.ToString();
                tbLettersRemaining.Text = m_gameState.LettersRemaining.Count.ToString();
                UpdatePicture();

                if (m_gameState.Players[m_playerID].HasTurn == null || m_gameState.CurrentWord == null)
                {
                    icLetters.IsEnabled = false;
                    btnGuess.IsEnabled = false;
                    btnHint.IsEnabled = false;
                    if (m_gameState.CurrentWord == null && gameHasEnded == false)
                    {
                        gameHasEnded = true;

                        Player winner = m_gameState.Players.OrderByDescending(p => p.LettersScore).FirstOrDefault();

                        MessageBox.Show(
                            string.Format(
                                "There are no more words left to be guessed. The winner is {0} with {1} points.",
                                winner.Name,
                                winner.LettersScore),
                            "Game Complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                // Call asynchronously
                Dispatcher.BeginInvoke(new UIUpdateDelegate(UpdateUI));
            }
        }

        #region Event Handlers
        private void Letter_Click(object sender, RoutedEventArgs e)
        {
            if (m_gameState.Players[m_playerID].HasTurn == true)
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
            if (m_playerID >= 0)
                m_gameState.LeaveGame(m_playerID);
        }

        private void btnHint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                m_gameState.CurrentWord.Hint,
                "Hint",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void btnGuess_Click(object sender, RoutedEventArgs e)
        {
            if (m_gameState.Players[m_playerID].HasTurn == true)
            {
                GuessWordWindow guessWindow = new GuessWordWindow(m_gameState);
                guessWindow.ShowDialog();
            }
        }
        #endregion

        #region Private Methods
        private void UpdatePicture()
        {
            int incorrectGuesses = m_gameState.Players[m_playerID].IncorrectGuesses;
            imgHangman.Source = new BitmapImage(new Uri(@"/images/HM_" + incorrectGuesses + ".png", UriKind.Relative));
        }
        #endregion
    }
}
