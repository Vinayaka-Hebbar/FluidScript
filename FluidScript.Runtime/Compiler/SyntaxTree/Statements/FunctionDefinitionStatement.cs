using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDefinitionStatement : FunctionDeclarationStatement
    {
        public readonly Metadata.FunctionPrototype Scope;

        internal readonly Reflection.DeclaredMethod Member;

        public BodyStatement Body { get; }

        internal FunctionDefinitionStatement(FunctionDeclaration declaration, BodyStatement body, Reflection.DeclaredMethod member) : base(declaration, StatementType.Function)
        {
            Body = body;
            Scope = declaration.Prototype;
            Member = member;
        }

        public override bool Equals(object obj)
        {
            return Name.Equals(obj);
        }

        public override string ToString()
        {
            return Declaration.ToString();
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            Body.GenerateCode(generator, info);
            if (info.ReturnTarget != null)
                generator.DefineLabelPosition(info.ReturnTarget);
            if (info.ReturnVariable != null)
                generator.LoadVariable(info.ReturnVariable);
        }

#if Runtime
        public override RuntimeObject Evaluate([Optional]RuntimeObject instance)
        {
            var reference = new Metadata.DynamicFunction(Member, instance, Member.DynamicInvoke);
            instance[Name] = reference;
            return reference;
        }
#endif

        public override int GetHashCode()
        {
            var hashCode = 1062545247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<FunctionDeclaration>.Default.GetHashCode(Declaration);
            hashCode = hashCode * -1521134295 + EqualityComparer<Statement>.Default.GetHashCode(Body);
            hashCode = hashCode * -1521134295 + NodeType.GetHashCode();
            return hashCode;
        }

#if Emit
        public override System.Reflection.MethodInfo Create()
        {
            System.Type returnType = null;
            if (ReturnTypeName.FullName != null)
                returnType = Compiler.Emit.TypeUtils.GetType(ReturnTypeName);
            System.Reflection.Emit.DynamicMethod dynamicMethod = new System.Reflection.Emit.DynamicMethod(Name, returnType,
                Arguments
                .Select(arg => Compiler.Emit.TypeUtils.GetType(arg.TypeName))
                .ToArray());
            var generator = new Compiler.Emit.ReflectionILGenerator(dynamicMethod.GetILGenerator(), false);
            var info = new Compiler.Emit.MethodOptimizationInfo(System.Type.GetType)
            {
                SyntaxTree = Body,
                FunctionName = Name,
                ReturnType = returnType
            };
            Member.Generate(generator, info);
            generator.Complete();
            return dynamicMethod.GetBaseDefinition();
        }
#endif
    }
}
