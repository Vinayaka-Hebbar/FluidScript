using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class NameExpression : Expression
    {
        public readonly string Name;
        public NameExpression(string name, ExpressionType opCode) : base(opCode)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return instance[Name];
        }

        public override RuntimeObject Evaluate(Metadata.Prototype prototype)
        {
            if (prototype is Metadata.FunctionPrototype funcProto)
            {
                var localVariable = funcProto.GetLocalVariable(Name);
                if (localVariable != null)
                    return localVariable.DefaultValue ?? RuntimeObject.Null;
            }
            var field = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<Reflection.DeclaredField>(prototype.GetMembers()), m => m.Name == Name);
            if (field != null)
                return field.DefaultValue ?? RuntimeObject.Null;
            return RuntimeObject.Null;
        }
#endif

        public static System.Type[] GetTypes(MethodBodyGenerator generator, System.Collections.Generic.IEnumerable<Expression> expressions)
        {
            return expressions.Select(arg => arg.ResultType(generator)).ToArray();
        }

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            var variable = generator.GetLocalVariable(Name);
            if (variable != null)
            {
                if (variable.Type == null)
                    throw new System.Exception(string.Concat("Use of undeclared variable ", variable));
                ResolvedType = variable.Type;
            }
            //find in the class level
            var member = generator.TypeGenerator.FindMember(Name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new System.Exception(string.Concat("Use of undeclared field ", field));
                    ResolvedType = field.FieldType;
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    ResolvedType = property.PropertyType;
                }
            }
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var variable = generator.GetLocalVariable(Name);
            if (variable != null)
            {
                if (variable.Type == null)
                    throw new System.Exception(string.Concat("Use of undeclared variable ", variable));
                ResolvedType = variable.Type;
                generator.LoadVariable(variable);
            }
            //find in the class level
           var member = generator.TypeGenerator.FindMember(Name).FirstOrDefault();
            if(member != null)
            {
                if(member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if(field.FieldType == null)
                        throw new System.Exception(string.Concat("Use of undeclared field ", field));
                    ResolvedType = field.FieldType;
                    generator.LoadField(field);
                }
                if(member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    ResolvedType = property.PropertyType;
                    generator.Call(property.GetGetMethod(true));
                }
            }
        }

        public void Invoke(MethodBodyGenerator generator, System.Collections.Generic.IEnumerable<Expression> arguments)
        {
            var types = GetTypes(generator, arguments);
            if (generator.TryGetMethod(Name, types, out System.Reflection.MethodBase method))
            {
                foreach (var item in arguments)
                {
                    item.GenerateCode(generator);
                }
                generator.Call(method);
            }
        }
#if Emit
        protected override void ResolveType(OptimizationInfo info)
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
                                ResolvedType = variable.Store.Type;
                                return;
                            }
                        }
                        break;
                    case Scopes.ScopeContext.Type:
                        if (scope is Scopes.ObjectScope type)
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
                    case Scopes.ScopeContext.Global:
                        break;
                    default:
                        break;
                }
                scope = scope.ParentScope;

            } while (scope != null);
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
                    case Scopes.ScopeContext.Type:
                        if (scope is Scopes.ObjectScope type)
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
                    case Scopes.ScopeContext.Global:
                        break;
                    default:
                        break;
                }
                scope = scope.ParentScope;

            } while (scope != null);
        }
#endif

    }
}
