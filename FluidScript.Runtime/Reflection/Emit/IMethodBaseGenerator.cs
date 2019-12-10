﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Reflection.Emit
{
    public interface IMethodBaseGenerator : IMemberGenerator
    {
        MethodBase MethodBase { get; }
        IEnumerable<ParameterInfo> Parameters { get; }
        Type[] ParameterTypes { get; }
        Statement SyntaxTree { get; }

        Type ReturnType { get; }

        TypeGenerator TypeGenerator { get; }
        Type GetType(string typeName);
    }
}