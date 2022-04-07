using System.Collections.Generic;
using System.Diagnostics;

namespace PushObject.Model
{

    [DebuggerDisplay("[TimeConjugation] time: {Time}")]
    public sealed class TimeConjugation
    {
        public string Id { get; }
        public string VerbId { get; }
        public string Time { get; }
        public ICollection<Conjugation> Conjugations { get; }
    }
}