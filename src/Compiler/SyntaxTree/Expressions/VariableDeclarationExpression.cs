using FluidScript.Compiler.Emit;
using FluidScript.Core;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : Expression
    {
        public readonly string Name;
        public readonly Scopes.Scope Scope;
        public readonly Reflection.DeclaredVariable Variable;

        public VariableDeclarationExpression(string name, Scopes.Scope scope, Reflection.DeclaredVariable variable) : base(ExpressionType.Declaration)
        {
            Name = name;
            Scope = scope;
            Variable = variable;

        }

        protected override void ResolveType(OptimizationInfo info)
        {
            Expression valueAtTop = Variable.ValueAtTop;
            if (valueAtTop == null)
                return;
            else
            {
                ResolvedPrimitiveType = valueAtTop.PrimitiveType(info);
                ResolvedType = valueAtTop.ResultType(info);
            }
        }

        public override RuntimeObject Evaluate()
        {
            if (Variable.ValueAtTop == null)
                return RuntimeObject.Null;
            return Variable.ValueAtTop.Evaluate();
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            //initialize
            if (Variable.ValueAtTop != null)
            {
                Variable.ValueAtTop.GenerateCode(generator, info);
            }
            if (Variable.Store == null)
            {
                Type type = Variable.ResolveType(info);
                if (type == null)
                {
                    type = ResultType(info);
                    Variable.ResolveType(type);
                }
                Variable.Store = generator.DeclareVariable(type, Name);
            }
            generator.StoreVariable(Variable.Store);
        }
    }
}
