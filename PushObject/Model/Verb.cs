using System.Collections.Generic;
using System.Diagnostics;
using Google.Cloud.Firestore;

namespace PushObject.Model
{

    [DebuggerDisplay("[Verb] infinitive: {Infinitive}")]
    [FirestoreData]
    public class Verb
    {
        [FirestoreProperty]
        public string Infinitive { get; set; }
        
        [FirestoreProperty]
        public int Group { get; set; }
        
        [FirestoreProperty]
        public ICollection<TimeConjugation> TimeConjugations { get; set; }
        public override string ToString()
        {
            return string.Format($"Verb: {Infinitive}");
        }
    }
}