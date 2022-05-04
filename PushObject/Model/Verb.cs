using System.Collections.Generic;
using System.Diagnostics;
using Google.Cloud.Firestore;

namespace PushObject.Model
{

    [DebuggerDisplay("[Verb] infinitive: {Infinitive}")]
    [FirestoreData]
    public class Verb
    {
        public string Infinitive { get; set; }
        public ICollection<TimeConjugation> TimeConjugations { get; set; }
        public override string ToString()
        {
            return string.Format($"Verb: {Infinitive}");
        }
    }
}