using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [OperationContract(IsOneWay = true)]
        void NewWord();

        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(Player player);

        [OperationContract(IsOneWay = true)]
        void ResetLetters();

        [OperationContract(IsOneWay = true)]
        void RemoveLetterFromPlay(char ch);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameState : IGameState
    {
        #region Properties
        public int WordsRemaining { get; private set; }
        public int WordsTotal { get; private set; }
        public List<char> LettersRemaining { get; private set; }
        public Word CurrentWord
        {
            get { return m_currentWord.Current; }
        }
        #endregion

        #region Members
        private StreamReader m_textReader;
        private List<Word> m_words;
        private IEnumerator<Word> m_currentWord;
        private Dictionary<Player, IClientCallback> m_dictPlayers;
        #endregion

        #region Constructor
        public GameState()
        {
            m_textReader = new StreamReader("WordsDatabase.txt");
            m_words = new List<Word>();
            m_dictPlayers = new Dictionary<Player, IClientCallback>();

            // Read words from text file and store them in memory
            while (!m_textReader.EndOfStream)
            {
                try
                {
                    string[] wordAndHint = m_textReader.ReadLine().Split(',');
                    m_words.Add(new Word(wordAndHint[0].ToUpperInvariant(), wordAndHint[1]));
                }
                catch (Exception) { }
            }

            // Populate the list of letters
            ResetLetters();

            m_currentWord = m_words.GetEnumerator();    // Starts BEFORE the first element in the list
        }
        #endregion

        #region Public Methods
        public void NewWord()
        {
            if (!m_currentWord.MoveNext())
                return;
        }

        public void RegisterPlayer(Player player)
        {
            IClientCallback callback = OperationContext.Current.GetCallbackChannel<IClientCallback>();
            m_dictPlayers.Add(player, callback);
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

        public void RemoveLetterFromPlay(char ch)
        {
            LettersRemaining.Remove(ch);
            NotifyClients();
        }
        #endregion

        #region Private Methods
        private void NotifyClients()
        {
            foreach (var player in m_dictPlayers)
                player.Value.UpdateUI();
        }
        #endregion
    }
}
