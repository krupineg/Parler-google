using System.Diagnostics;

namespace PushObject.Model
{

    [DebuggerDisplay("[Conjugation] pronoun: {Male}/{Female}/{Combined}, party: {Party} value: {Value}")]
    public class Conjugation
    {
        public string Id { get; set; }
        public string TimeConjugationId { get; set; }
        public string Male { get; set; }
        public string Female { get; set; }
        public string Combined { get; set; }
        public PartyFlag Party { get; set; }
        public string Value { get; set; }
    }
}