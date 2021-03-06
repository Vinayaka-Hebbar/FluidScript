﻿using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ValueAccessExpression : Expression
    {
        public readonly string Name;

        public ValueAccessExpression(string name, ExpressionType opCode) : base(opCode)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

}
