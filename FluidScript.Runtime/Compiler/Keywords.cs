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
                {"class", IdentifierType.Class },
                {"public", IdentifierType.Public },
                {"private", IdentifierType.Private },
                {"get",  IdentifierType.Get },
                {"set", IdentifierType.Set },
                {"new", IdentifierType.New },
                {"this", IdentifierType.This },
                {"true",IdentifierType.True },
                {"false",IdentifierType.False },
                {"null", IdentifierType.Null },
                {"undefined", IdentifierType.Undefined },
                {"return", IdentifierType.Return },
                {"var", IdentifierType.Var },
                {"const", IdentifierType.Const },
                {"impl", IdentifierType.Implement },
                {"func", IdentifierType.Function },
                {"lamda", IdentifierType.Lamda },
                {"if", IdentifierType.If},
                {"else", IdentifierType.Else },
                {"while",IdentifierType.While },
                {"do", IdentifierType.Do },
                {"for", IdentifierType.For },
                {"loop", IdentifierType.Loop },
                {"continue",IdentifierType.Continue },
                {"switch", IdentifierType.Switch },
                {"break", IdentifierType.Break },
                {"throw", IdentifierType.Throw },
            };
        }
        

        public static bool TryGetIdentifier(string name, out IdentifierType type)
        {
            return keywords.TryGetValue(name, out type);
        }
    }
}
