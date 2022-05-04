using System.Collections.Generic;
using System.Diagnostics;
using Google.Cloud.Firestore;

namespace PushObject.Model
{
    [FirestoreData]
    [DebuggerDisplay("[TimeConjugation] time: {Time}")]
    public sealed class TimeConjugation
    {
        [FirestoreProperty]
        public string Time { get; set; }
        [FirestoreProperty]
        public ICollection<Conjugation> Conjugations { get; set; }
    }
}