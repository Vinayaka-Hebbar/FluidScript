using FluidScript.Compiler.Emit;
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


        public override PrimitiveType PrimitiveType()
        {
            Expression valueAtTop = Variable.ValueAtTop;
            if (valueAtTop == null)
                return FluidScript.PrimitiveType.Null;
            return valueAtTop.PrimitiveType();
        }

        public override object GetValue()
        {
            if (Variable.ValueAtTop == null)
                return null;
            return Variable.ValueAtTop.GetValue();
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
                Type type = Variable.GetType(info);
                if (type == null && Variable.ValueAtTop != null)
                {
                    type = Variable.ValueAtTop.ResultType();
                    ResolvedType = type;
                    Variable.ResolveType(type);
                }
                Variable.Store = generator.DeclareVariable(type, Name);
            }
            generator.StoreVariable(Variable.Store);
        }
    }
}
