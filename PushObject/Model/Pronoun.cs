using System.Diagnostics;

namespace PushObject.Model
{
    [DebuggerDisplay("[Pronoun] male: {Male}, female: {Female}, combined: {Combined}, party: {Party}")]
    public class Pronoun
    {
        public string Id { get; set; } 
        public string Male { get; set; }
        public string Female { get; set; }
        public string Combined { get; set; }
        public PartyFlag Party { get; set;}
    }
}