using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
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
        public List<char> LettersGuessed { get; }
        [DataMember]
        public bool HasTurn { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
