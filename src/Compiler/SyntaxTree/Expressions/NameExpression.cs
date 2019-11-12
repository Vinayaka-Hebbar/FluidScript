using System;
using System.Linq;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class NameExpression : Expression
    {
        public readonly string Name;
        public readonly Metadata.Scope Scope;
        public NameExpression(string name, Metadata.Scope scope, ExpressionType opCode) : base(opCode)
        {
            Name = name;
            Scope = scope;
        }

        public override string ToString()
        {
            return Name;
        }

        protected override void ResolveType(OptimizationInfo info)
        {
            Metadata.Scope scope = Scope;
            do
            {
                switch (scope.Context)
                {
                    case Metadata.ScopeContext.Local:
                        if (scope.CanDeclareVariables && scope is Metadata.DeclarativeScope)
                        {
                            var declarative = (Metadata.DeclarativeScope)scope;
                            var variable = declarative.GetVariable(Name);
                            if (variable != null)
                            {
                                ResolvedPrimitiveType = variable.PrimitiveType;
                                ResolvedType = variable.Store.Type;
                                return;
                            }
                        }
                        break;
                    case Metadata.ScopeContext.Type:
                        if (scope is Metadata.ObjectScope type)
                        {
                            var memeber = type.GetMember(Name).FirstOrDefault(item => item.IsMethod == false);
                            if (memeber != null)
                            {
                                ResolvedPrimitiveType = memeber.Declaration.PrimitiveType;
                                ResolvedType = memeber.Declaration.ResolvedType;
                                return;
                            }
                        }
                        break;
                    case Metadata.ScopeContext.Global:
                        break;
                    default:
                        break;
                }
                scope = scope.ParentScope;

            } while (scope != null);
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            Metadata.Scope scope = Scope;
            do
            {
                switch (scope.Context)
                {
                    case Metadata.ScopeContext.Local:
                        if (scope.CanDeclareVariables && scope is Metadata.DeclarativeScope)
                        {
                            var declarative = (Metadata.DeclarativeScope)scope;
                            var variable = declarative.GetVariable(Name);
                            if (variable != null)
                            {
                                ResolvedType = variable.ResolveType(info);
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
                    case Metadata.ScopeContext.Type:
                        if (scope is Metadata.ObjectScope type)
                        {
                            var memeber = type.GetMember(Name).FirstOrDefault(item => item.IsMethod == false);
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
                    case Metadata.ScopeContext.Global:
                        break;
                    default:
                        break;
                }
                scope = scope.ParentScope;

            } while (scope != null);
        }
    }
}
