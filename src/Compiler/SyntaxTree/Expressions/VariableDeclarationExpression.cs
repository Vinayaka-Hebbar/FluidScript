using System;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : Expression
    {
        public readonly string Name;
        protected readonly Scopes.Scope Scope;
        protected readonly DeclaredVariable Variable;

        public VariableDeclarationExpression(string name, Scopes.Scope scope, DeclaredVariable variable) : base(ExpressionType.Declaration)
        {
            Name = name;
            Scope = scope;
            Variable = variable;
        }

        public override PrimitiveType ResultType
        {
            get
            {
                Expression valueAtTop = Variable.ValueAtTop;
                if (valueAtTop == null)
                    return PrimitiveType.Null;
                return valueAtTop.ResultType;
            }
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            //initialize
            if (Variable.ValueAtTop != null)
            {
                Variable.ValueAtTop.GenerateCode(generator, info);
            }
            if (Variable.Store == null)
            {
                Type type = Variable.GetType(info.TypeProvider);
                if (type == null && Variable.ValueAtTop != null)
                {
                    type = Variable.ValueAtTop.Type;
                    ResolvedType = type;
                    Variable.ResolveType(type);
                }
                Variable.Store = generator.DeclareVariable(type, Name);
            }
            generator.StoreVariable(Variable.Store);
        }
    }
}
