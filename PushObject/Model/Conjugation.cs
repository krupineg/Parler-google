using System.Diagnostics;
using Google.Cloud.Firestore;

namespace PushObject.Model
{
    [FirestoreData]
    [DebuggerDisplay("[Conjugation] pronoun: {Male}/{Female}/{Combined}, party: {Party} value: {Value}")]
    public class Conjugation
    {
        public string Male { get; set; }
        public string Female { get; set; }
        public string Combined { get; set; }
        public int Party { get; set; }
        public string Value { get; set; }
    }
}