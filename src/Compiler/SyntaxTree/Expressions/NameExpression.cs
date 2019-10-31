using System;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class NameExpression : Expression
    {
        public readonly string Name;
        public readonly Scopes.Scope Scope;
        public NameExpression(string name, Scopes.Scope scope, ExpressionType opCode) : base(opCode)
        {
            Name = name;
            Scope = scope;
        }

        public override string ToString()
        {
            return Name;
        }

        public override PrimitiveType ResultType
        {
            get
            {
                Scopes.Scope scope = Scope;
                do
                {
                    if (scope.CanDeclareVariables && scope is Scopes.DeclarativeScope)
                    {
                        var declarative = (Scopes.DeclarativeScope)scope;
                        var variable = declarative.GetVariable(Name);
                        if (variable != null)
                        {
                            return variable.PrimitiveType;
                        }
                    }
                    scope = scope.ParentScope;

                } while (scope != null);
                return PrimitiveType.Any;
            }
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            Scopes.Scope scope = Scope;
            do
            {
                if (scope.CanDeclareVariables && scope is Scopes.DeclarativeScope)
                {
                    var declarative = (Scopes.DeclarativeScope)scope;
                    var variable = declarative.GetVariable(Name);
                    if (variable != null)
                    {
                        ResolvedType = variable.GetType(info.TypeProvider);
                        if (variable.VariableType == Reflection.VariableType.Argument)
                        {
                            generator.LoadArgument(variable.Index);
                            return;
                        }
                        if (variable.Store == null)
                            variable.Store = generator.DeclareVariable(ResolvedType, Name);
                        generator.LoadVariable(variable.Store);
                        //to do result type
                        return;
                    }
                }
                scope = scope.ParentScope;

            } while (scope != null);
        }
    }
}
