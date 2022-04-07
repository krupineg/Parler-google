using System.Collections.Generic;
using System.Diagnostics;

namespace PushObject.Model
{

    [DebuggerDisplay("[Verb] infinitive: {Infinitive}")]
    public class Verb : Identifiable
    {
        public string Infinitive { get; }
        public ICollection<TimeConjugation> TimeConjugations { get; }
        public override string ToString()
        {
            return string.Format($"{Id} : {Infinitive}");
        }
    }
}