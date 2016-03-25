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

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                DuplexChannelFactory<IGameState> gameStateFactory =
                    new DuplexChannelFactory<IGameState>(this, "GameState");
                m_gameState = gameStateFactory.CreateChannel();

                Player p = new Player();
                p.Name = "Italo";
                m_gameState.RegisterPlayer(p);

                icLetters.ItemsSource = m_gameState.LettersRemaining;
                m_gameState.NewWord();
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
                icLetters.ItemsSource = m_gameState.LettersRemaining;
            }
            else
            {
                // Call asynchronously
                Dispatcher.BeginInvoke(new UIUpdateDelegate(UpdateUI));
            }
        }

        private void Letter_Click(object sender, RoutedEventArgs e)
        {
            char letter = (char)((Button)e.Source).Content;
            m_gameState.RemoveLetterFromPlay(letter);
        }
    }
}
