using System.Collections.Generic;
using System.Diagnostics;

namespace PushObject.Model
{

    [DebuggerDisplay("[TimeConjugation] time: {Time}")]
    public sealed class TimeConjugation
    {
        public string Time { get; set; }
        public ICollection<Conjugation> Conjugations { get; set; }
    }
}