using System.Collections.Generic;

namespace FluidScript.Compiler.Lexer
{
    public static class Keywords
    {
        public const string From = "from";
        public const string Extends = "extends";

        public const string Implements = "implements";

        static readonly IDictionary<string, IdentifierType> keywords;

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
                {"super", IdentifierType.Super },
                {"import", IdentifierType.Import },
                {"true",IdentifierType.True },
                {"false",IdentifierType.False },
                {"NaN",IdentifierType.NaN },
                {"null", IdentifierType.Null },
                {"undefined", IdentifierType.Undefined },
                {"return", IdentifierType.Return },
                {"var", IdentifierType.Var },
                {"val", IdentifierType.Val },
                {"impl", IdentifierType.Implement },
                {"func", IdentifierType.Function },
                {"ctor", IdentifierType.Ctor },
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
                {"sizeof", IdentifierType.SizeOf},
                {From, IdentifierType.From }
            };
        }


        public static bool TryGetIdentifier(string name, out IdentifierType value)
        {
            return keywords.TryGetValue(name, out value);
        }

        public static bool Match(string name, IdentifierType expected)
        {
            return keywords.TryGetValue(name, out IdentifierType type) ? type == expected : false;
        }
    }
}
