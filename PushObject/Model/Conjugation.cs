using System.Diagnostics;
using Google.Cloud.Firestore;

namespace PushObject.Model
{
    [FirestoreData]
    [DebuggerDisplay("[Conjugation] pronoun: {Male}/{Female}/{Combined}, party: {Party} value: {Value}")]
    public class Conjugation
    {
        [FirestoreProperty]
        public string Male { get; set; }
        
        [FirestoreProperty]
        public string Female { get; set; }
        
        [FirestoreProperty]
        public string Combined { get; set; }
        
        [FirestoreProperty]
        public int Party { get; set; }
        
        [FirestoreProperty]
        public string Value { get; set; }
    }
}