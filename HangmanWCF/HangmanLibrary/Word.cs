using System.Runtime.Serialization;

namespace HangmanLibrary
{
    [DataContract]
    public class Word
    {
        [DataMember]
        public string WordString { get; private set; }
        [DataMember]
        public string Hint { get; private set; }
        
        public Word(string wordText, string hint)
        {
            WordString = wordText;
            Hint = hint;
        }
    }
}
