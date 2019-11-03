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

        public override PrimitiveType PrimitiveType(Emit.MethodOptimizationInfo info)
        {
            if (ResolvedPrimitiveType == FluidScript.PrimitiveType.Any)
            {
                Scopes.Scope scope = Scope;
                do
                {
                    switch (scope.Context)
                    {
                        case Scopes.ScopeContext.Local:
                            if (scope.CanDeclareVariables && scope is Scopes.DeclarativeScope)
                            {
                                var declarative = (Scopes.DeclarativeScope)scope;
                                var variable = declarative.GetVariable(Name);
                                if (variable != null)
                                {
                                    ResolvedPrimitiveType = variable.PrimitiveType;
                                }
                            }
                            break;
                        case Scopes.ScopeContext.Type:
                            if (scope is Scopes.ObjectScope type)
                            {
                                var memeber = type.GetMember(Name);
                                if (memeber != null)
                                {
                                    ResolvedPrimitiveType = memeber.Declaration.PrimitiveType;
                                    break;
                                }
                            }
                            break;
                        case Scopes.ScopeContext.Global:
                            break;
                        default:
                            break;
                    }
                    scope = scope.ParentScope;

                } while (scope != null);
            }
            return ResolvedPrimitiveType;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            Scopes.Scope scope = Scope;
            do
            {
                switch (scope.Context)
                {
                    case Scopes.ScopeContext.Local:
                        if (scope.CanDeclareVariables && scope is Scopes.DeclarativeScope)
                        {
                            var declarative = (Scopes.DeclarativeScope)scope;
                            var variable = declarative.GetVariable(Name);
                            if (variable != null)
                            {
                                ResolvedType = variable.GetType(info);
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
                        break;
                    case Scopes.ScopeContext.Type:
                        if (scope is Scopes.ObjectScope type)
                        {
                            var memeber = type.GetMember(Name);
                            if (memeber != null)
                            {
                                if (memeber.IsGenerated == false)
                                {
                                    memeber.Generate(info);
                                }

                                ResolvedType = memeber.ResolvedType;
                                if (memeber.MemberType == System.Reflection.MemberTypes.Field)
                                {
                                    generator.LoadArgument(0);
                                    generator.LoadField((System.Reflection.FieldInfo)memeber.Info);
                                    return;
                                }
                                //to do result type
                                return;
                            }
                        }
                        break;
                    case Scopes.ScopeContext.Global:
                        break;
                    default:
                        break;
                }
                scope = scope.ParentScope;

            } while (scope != null);
        }
    }
}
