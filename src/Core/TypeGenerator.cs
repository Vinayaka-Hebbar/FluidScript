using FluidScript.Compiler;
using FluidScript.Compiler.Reflection;

namespace FluidScript.Core
{
    public class TypeGenerator
    {
        public readonly TypeInfo Type;

        public TypeGenerator(TypeInfo type)
        {
            Type = type;
        }

        public MethodInfo GenerateMethod(string name, ParameterInfo[] parameters, TypeInfo returnType, string code)
        {
            var methodInfo = Type.DeclareMethod(name, parameters, returnType, System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.HideBySig);
            var visitor = new SyntaxVisitor(code, new ParserSettings());
            var statement = visitor.VisitStatement(CodeScope.Local);
            var body = methodInfo.GetMethodBody();
            body.Add(statement);
            return methodInfo;
        }

        public System.Type Generate()
        {
            if (Type.IsGenerated)
                return Type.RuntimeType();
            var module = Type.Module;
            var assemblyBuilder = new AssemblyBuilder(module.Assembly, System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineModule(module, false);
            Type.Generate(moduleBuilder);
            return Type.RuntimeType();
        }
    }
}
