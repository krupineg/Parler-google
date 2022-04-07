using System;

namespace PushObject.Model
{
    public static class PartyFlags
    {
        public static PartyFlag Verify(this PartyFlag partyFlag)
        {
            if (partyFlag.HasFlag(PartyFlag.First | PartyFlag.Second) ||
                partyFlag.HasFlag(PartyFlag.Second | PartyFlag.Third) || 
                partyFlag.HasFlag(PartyFlag.Third | PartyFlag.First) ||
                partyFlag == PartyFlag.Plural)
            {
                throw new ArgumentException("Impossible party");
            }
            return partyFlag;
        }
    }
}