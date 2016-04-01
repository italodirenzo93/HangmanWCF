using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace HangmanLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember]
        public int LettersScore { get; internal set; }
        [DataMember]
        public List<char> LettersGuessed { get; private set; }
        [DataMember]
        public int IncorrectGuesses { get; internal set; }
        [DataMember]
        public bool? HasTurn { get; internal set; }
        [DataMember]
        public string Name { get; private set; }

        // NOTE: This property is for display in the UI. Not an index for within an array.
        [DataMember]
        public int PlayerIndex { get; private set; }
        
        internal IClientCallback Callback { get; private set; }

        private static int m_pIndex = 1;

        public Player(string name)
        {
            Name = name;
            PlayerIndex = m_pIndex++;
            LettersGuessed = new List<char>();
            IncorrectGuesses = 0;
            Callback = OperationContext.Current.GetCallbackChannel<IClientCallback>();
        }

        internal void Reset()
        {
            LettersGuessed.Clear();
            IncorrectGuesses = 0;
            HasTurn = false;
        }
    }
}
