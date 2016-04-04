using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.IO;

namespace HangmanLibrary
{
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

        List<char> LetterTiles { [OperationContract]get; }

        List<Player> Players { [OperationContract]get; }

        string UpdateMessage { [OperationContract]get; }

        [OperationContract]
        int RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void LeaveGame(int playerID);

        [OperationContract(IsOneWay = true)]
        void ResetLetters();

        [OperationContract(IsOneWay = true)]
        void GuessLetter(char ch);

        [OperationContract(IsOneWay = true)]
        void GuessWord(string word);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameState : IGameState
    {
        #region Properties
        public int WordsRemaining
        {
            get
            {
                int current_index = 0;
                if (CurrentWord != null)
                {
                    foreach (var w in m_words)
                    {
                        if (w.WordString != CurrentWord.WordString)
                            ++current_index;
                        else
                            break;
                    }
                }
                    
                return m_words.Count - current_index;
            }
        }
        public int WordsTotal { get; private set; }
        public List<char> LettersRemaining { get; private set; }
        public List<char> LetterTiles { get; private set; }
        public List<Player> Players { get; private set; }
        public string UpdateMessage { get; private set; }
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
        private const int MAX_INCORRECT_GUESSES = 6;
        private const int POINTS_PER_WORD_GUESSED = 5;
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
            p.HasTurn = false;
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
            if (p.HasTurn == true)
                QueueNextTurn();

            Players.Remove(p);

            if (Players.Count > 0)
                NotifyClients();
        }

        public void GuessLetter(char ch)
        {
            UpdateMessage = string.Empty;

            // Remove that letter from play
            LettersRemaining.Remove(ch);

            // Update player information
            Player p = Players[m_currentPlayerIndex];

            p.LettersGuessed.Add(ch);
            if (CurrentWord.WordString.Contains(ch.ToString()))
            {
                for (int i = 0; i < CurrentWord.WordString.Length; i++)
                {
                    if (CurrentWord.WordString[i] == ch)
                        LetterTiles[i] = ch;
                }

                p.LettersScore += 1;

                // Check for a win
                if (new string(LetterTiles.ToArray()) == CurrentWord.WordString)
                {
                    UpdateMessage = "Word was found! " + CurrentWord.WordString;
                    NewWord();
                    ResetLetters();
                    foreach (Player pl in Players)
                        pl.Reset();
                }
            }
            else
            {
                p.IncorrectGuesses += 1;
                if (p.IncorrectGuesses == MAX_INCORRECT_GUESSES)
                    p.HasTurn = null;
            }
            
            QueueNextTurn();
            NotifyClients();
        }

        public void GuessWord(string word)
        {
            Player p = Players[m_currentPlayerIndex];
            if (word == CurrentWord.WordString)
            {
                p.LettersScore += POINTS_PER_WORD_GUESSED;
                UpdateMessage = string.Format("{0} guessed the word! It was {1}.", p.Name, CurrentWord.WordString);
                NewWord();
                ResetLetters();

                p.LettersGuessed.Clear();
                foreach(var player in Players)
                    player.IncorrectGuesses = 0;
            }
            else
            {
                UpdateMessage = string.Format("{0} attempted to guess the word. The word is not {1}", p.Name, word);
                p.IncorrectGuesses += 1;
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
            else
            {
                LetterTiles = new List<char>(CurrentWord.WordString.Length);
                foreach (char letter in CurrentWord.WordString)
                {
                    if (letter == ' ')
                        LetterTiles.Add(' ');
                    else
                        LetterTiles.Add('_');
                }
            }
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

        private void NotifyClients()
        {
            foreach (Player p in Players)
                p.Callback.UpdateUI();
        }

        private void QueueNextTurn()
        {
            // No longer the last player's turn
            if (m_currentPlayerIndex != -1 && Players[m_currentPlayerIndex].HasTurn.HasValue)
                Players[m_currentPlayerIndex].HasTurn = false;

            // If all players have gone, back to player 1
            if (m_currentPlayerIndex == Players.Count - 1)
                m_currentPlayerIndex = -1;

            m_currentPlayerIndex += 1;

            // Make sure we don't move out of range
            if (m_currentPlayerIndex == Players.Count)
                m_currentPlayerIndex = 0;

            // Make sure the player is not "out"
            while (Players[m_currentPlayerIndex].HasTurn == null)
            {
                m_currentPlayerIndex += 1;
                // If all players have gone, back to player 1
                if (m_currentPlayerIndex == Players.Count)
                    m_currentPlayerIndex = 0;
            }

            Players[m_currentPlayerIndex].HasTurn = true;
        }
        #endregion
    }
}
