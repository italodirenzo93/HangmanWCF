using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HangmanLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public int LettersGuessedCorrectly { get; set; }
        [DataMember]
        public List<char> LettersGuessed { get; private set; }
        [DataMember]
        public bool HasTurn { get; set; }
        [DataMember]
        public string Name { get; set; }

        public Player(string name)
        {
            Name = name;
            LettersGuessed = new List<char>();
        }
    }
}
