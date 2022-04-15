using System.Diagnostics;

namespace PushObject.Model.V2
{

    [DebuggerDisplay("[Conjugation Flat] pronoun: {Infinitive} of {Time}: {Male}/{Female}/{Combined}, party: {Party} value: {Value}")]
    public class ConjugationFlat
    {  
        public long VerbIndex { get; set; }
        public long ConjugationIndex { get; set; }
        public string Infinitive { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string Male { get; set; }
        public string Female { get; set; }
        public string Combined { get; set; }
        public long Party { get; set; }
        public string Value { get; set; }
    }
}