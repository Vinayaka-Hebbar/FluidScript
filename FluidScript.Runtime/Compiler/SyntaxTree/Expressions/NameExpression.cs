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


        protected override void ResolveType(MethodBodyGenerator generator)
        {
            var variable = generator.GetLocalVariable(Name);
            if (variable != null)
            {
                if (variable.Type == null)
                    throw new System.Exception(string.Concat("Use of undeclared variable ", variable));
                ResolvedType = variable.Type;
                return;
            }
            Reflection.ParameterInfo arg = generator.MethodGenerator.Parameters.FirstOrDefault(para => para.Name == Name);
            if (arg.Name != null)
            {
                ResolvedType = generator.MethodGenerator.ParameterTypes[arg.Index];
                return;
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
                return;
            }
            Reflection.ParameterInfo arg = generator.MethodGenerator.Parameters.FirstOrDefault(para => para.Name == Name);
            if (arg.Name != null)
            {
                generator.LoadArgument(generator.MethodGenerator.IsStatic ? arg.Index : arg.Index + 1);
                ResolvedType = generator.MethodGenerator.ParameterTypes[arg.Index];
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
                    if (field.IsStatic == false)
                        generator.LoadArgument(0);
                    generator.LoadField(field);
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    ResolvedType = property.PropertyType;
                    System.Reflection.MethodInfo method = property.GetGetMethod(true);
                    if (method.IsStatic == false)
                        generator.LoadArgument(0);
                    generator.Call(method);
                }
                return;
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
