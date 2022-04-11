using System.Collections.Generic;
using System.Diagnostics;

namespace PushObject.Model
{

    [DebuggerDisplay("[Verb] infinitive: {Infinitive}")]
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