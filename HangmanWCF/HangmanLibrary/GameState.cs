using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.IO;

namespace HangmanLibrary
{
    [ServiceContract]
    public interface IClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateUI();
    }

    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    public interface IGameState
    {
        int WordsTotal { [OperationContract]get; }

        int WordsRemaining { [OperationContract]get; }

        Word CurrentWord { [OperationContract]get; }

        List<char> LettersRemaining { [OperationContract]get; }

        List<Player> Players { [OperationContract]get; }

        [OperationContract]
        int RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void LeaveGame(int playerID);

        [OperationContract(IsOneWay = true)]
        void ResetLetters();

        [OperationContract(IsOneWay = true)]
        void GuessLetter(char ch);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameState : IGameState
    {
        #region Properties
        public int WordsRemaining { get; private set; }
        public int WordsTotal { get; private set; }
        public List<char> LettersRemaining { get; private set; }
        public List<Player> Players { get; private set; }
        public Word CurrentWord
        {
            get { return m_currentWord.Current; }
        }
        #endregion

        #region Members
        private List<Word> m_words;
        private IEnumerator<Word> m_currentWord;
        private int m_currentPlayerIndex;

        private const int MAX_PLAYERS = 4;
        #endregion

        #region Constructor
        public GameState()
        {
            m_words = new List<Word>();
            Players = new List<Player>();

            // Read words from text file and store them in memory
            using (StreamReader reader = new StreamReader("WordsDatabase.txt"))
            {
                while (!reader.EndOfStream)
                {
                    try
                    {
                        string[] wordAndHint = reader.ReadLine().Split(',');
                        m_words.Add(new Word(wordAndHint[0].ToUpperInvariant(), wordAndHint[1]));
                    }
                    catch (Exception)
                    {
                        continue;   // Just ignore it
                    }
                }
            }

            // Populate the list of letters
            ResetLetters();

            m_currentWord = m_words.GetEnumerator();    // Starts BEFORE the first element in the list
            m_currentPlayerIndex = -1;

            // Start the game with a new word
            NewWord();
        }
        #endregion

        #region Public Methods
        public int RegisterPlayer(string playerName)
        {
            if (Players.Count >= MAX_PLAYERS)
                return -1;

            Player p = new Player(playerName);
            Players.Add(p);

            // If they are the first one joining, it's their turn
            if (Players.Count == 1)
                QueueNextTurn();

            NotifyClients();
            return Players.IndexOf(p);
        }

        public void LeaveGame(int playerID)
        {
            Player p = Players[playerID];
            if (p.HasTurn)
                QueueNextTurn();

            Players.Remove(p);

            if (Players.Count > 0)
                NotifyClients();
        }

        public void ResetLetters()
        {
            LettersRemaining = new List<char>
            {   'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N',
                'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z'
            };
            NotifyClients();
        }

        public void GuessLetter(char ch)
        {
            // Remove that letter from play
            LettersRemaining.Remove(ch);

            // Update player information
            Player p = Players[m_currentPlayerIndex];

            p.LettersGuessed.Add(ch);
            if (CurrentWord.WordString.Contains(ch.ToString()))
            {
                p.LettersGuessedCorrectly += 1;
            }

            QueueNextTurn();
            NotifyClients();
        }
        #endregion

        #region Private Methods
        private void NewWord()
        {
            if (!m_currentWord.MoveNext())
                return;
        }

        private void NotifyClients()
        {
            foreach (Player p in Players)
                p.Callback.UpdateUI();
        }

        private void QueueNextTurn()
        {
            // No longer the last player's turn
            if (m_currentPlayerIndex != -1)
                Players[m_currentPlayerIndex].HasTurn = false;

            // If all players have gone, back to player 1
            if (m_currentPlayerIndex == Players.Count - 1)
                m_currentPlayerIndex = -1;

            m_currentPlayerIndex += 1;
            Players[m_currentPlayerIndex].HasTurn = true;
        }
        #endregion
    }
}
