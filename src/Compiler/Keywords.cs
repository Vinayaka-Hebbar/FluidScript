using System.Collections.Generic;

namespace FluidScript.Compiler
{
    internal sealed class Keywords
    {
        internal static readonly IDictionary<string, IdentifierType> identifiers;

        public IDictionary<string, IdentifierType> Identifiers { get; }

        static Keywords()
        {
            identifiers = new Dictionary<string, IdentifierType>
            {
                {"new", IdentifierType.New },
                {"this", IdentifierType.This },
                {"true",IdentifierType.True },
                {"false",IdentifierType.False },
                {"out", IdentifierType.Return },
                {"return", IdentifierType.Return },
                {"var", IdentifierType.Var },
                {"function", IdentifierType.Function },
                {"lamda", IdentifierType.Lamda },
                {"if", IdentifierType.If},
                {"else", IdentifierType.Else },
                {"while",IdentifierType.While },
                {"do", IdentifierType.Do },
                {"for", IdentifierType.For },
                {"continue",IdentifierType.Continue },
                {"switch", IdentifierType.Switch },
                {"break", IdentifierType.Break },
                {"throw", IdentifierType.Throw },
                {"class", IdentifierType.Class },
                {"public", IdentifierType.Public },
                {"private", IdentifierType.Private }
            };
        }

        public Keywords()
        {
            Identifiers = new Dictionary<string, IdentifierType>(identifiers);
        }

        public bool TryGetIdentifier(string name, out IdentifierType type)
        {
            return Identifiers.TryGetValue(name, out type);
        }
    }
}
