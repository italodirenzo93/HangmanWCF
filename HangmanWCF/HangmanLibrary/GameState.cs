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
    public interface IGameState
    {
        int WordsTotal { [OperationContract]get; }
        int WordsRemaining { [OperationContract]get; }
        Word CurrentWord { [OperationContract]get; }
        [OperationContract]
        void NewWord();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GameState : IGameState
    {
        #region Properties
        public int WordsRemaining { get; private set; }
        public int WordsTotal { get; private set; }
        public Word CurrentWord
        {
            get { return m_currentWord.Current; }
        }
        #endregion

        #region Members
        private StreamReader m_textReader;
        private List<Word> m_words;
        private IEnumerator<Word> m_currentWord;

        #endregion

        #region Constructor
        public GameState()
        {
            m_textReader = new StreamReader("WordsDatabase.txt");
            m_words = new List<Word>();

            while (!m_textReader.EndOfStream)
            {
                try
                {
                    string[] wordAndHint = m_textReader.ReadLine().Split(',');
                    m_words.Add(new Word(wordAndHint[0].ToUpperInvariant(), wordAndHint[1]));
                }
                catch (Exception) { }
            }

            m_currentWord = m_words.GetEnumerator();    // Starts BEFORE the first element in the list
        }
        #endregion

        #region Public Methods
        public void NewWord()
        {
            if (!m_currentWord.MoveNext())
                return;
        }
        #endregion
    }
}
