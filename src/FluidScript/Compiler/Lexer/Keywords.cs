using System.Collections.Generic;

namespace FluidScript.Compiler.Lexer
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
                {"static", IdentifierType.Static },
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
                {"val", IdentifierType.Val },
                {"impl", IdentifierType.Implement },
                {"func", IdentifierType.Function },
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
                {"sizeof", IdentifierType.SizeOf}
            };
        }


        public static bool TryGetIdentifier(string name, out IdentifierType type)
        {
            return keywords.TryGetValue(name, out type);
        }

        internal static bool Match(string name, IdentifierType expected)
        {
            return keywords.TryGetValue(name, out IdentifierType type) ? type == expected : false;
        }
    }
}
