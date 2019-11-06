using System.Collections.Generic;

namespace FluidScript.Compiler
{
    internal static class Keywords
    {
        internal static readonly IDictionary<string, IdentifierType> keywords;

        static Keywords()
        {
            keywords = new Dictionary<string, IdentifierType>
            {
                {"new", IdentifierType.New },
                {"this", IdentifierType.This },
                {"true",IdentifierType.True },
                {"false",IdentifierType.False },
                {"null", IdentifierType.Null },
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
        

        public static bool TryGetIdentifier(string name, out IdentifierType type)
        {
            return keywords.TryGetValue(name, out type);
        }
    }
}
