﻿using FluidScript.Reflection;
using FluidScript.Reflection.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayTypeSyntax : TypeSyntax
    {
        public readonly TypeSyntax ElementType;

        public readonly Expression[] Ranks;

        public ArrayTypeSyntax(TypeSyntax elementType, Expression[] sizes)
        {
            ElementType = elementType;
            Ranks = sizes;
        }

        public override Type GetType(ITypeProvider provider)
        {
            return ElementType.GetType(provider).MakeArrayType();
        }

        public override ITypeInfo GetTypeInfo()
        {
            return ElementType.GetTypeInfo().MakeArrayType();
        }

        public override string ToString()
        {
            return string.Concat(ElementType.ToString(), "[", string.Join(",", System.Linq.Enumerable.Select(Ranks, size => size.ToString())), "]");
        }
    }
}