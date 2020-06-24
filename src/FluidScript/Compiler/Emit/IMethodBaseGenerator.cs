using FluidScript.Compiler.SyntaxTree;
using System;
using System.Reflection;

namespace FluidScript.Compiler.Emit
{
    public interface IMethodBaseGenerator : IMemberGenerator
    {
        MethodBase MethodBase { get; }
        ParameterInfo[] Parameters { get; }
        Statement SyntaxBody { get; }
        Type ReturnType { get; }

        CallingConventions CallingConvention { get; }

        IProgramContext Context { get; set; }
    }
}