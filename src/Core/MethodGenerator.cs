using FluidScript.Compiler.Scopes;
using System.Linq;

namespace FluidScript.Core
{
    public class MethodGenerator
    {
        public MethodGenerator(ScriptEngine scriptEngine, IScriptSource source, ObjectScope objectScope)
        {
            this.SyntaxVisitor = new Compiler.SyntaxVisitor(source, objectScope, scriptEngine.Settings);
        }

        public Compiler.SyntaxVisitor SyntaxVisitor { get; }

        public System.Reflection.MethodInfo Generate()
        {
            SyntaxVisitor.Reset();
            SyntaxVisitor.MoveNext();
            var name=  SyntaxVisitor.GetName();
            if (name == "function")
            {
                var statement = SyntaxVisitor.VisitFunctionDefinition();
                System.Type returnType = null;
                if(statement.ReturnTypeName.FullName != null)
                returnType = Compiler.Emit.TypeUtils.GetType(statement.ReturnTypeName);
                System.Reflection.Emit.DynamicMethod dynamicMethod = new System.Reflection.Emit.DynamicMethod(statement.Name, returnType, 
                    statement.Arguments
                    .Select(arg => Compiler.Emit.TypeUtils.GetType(arg.TypeName))
                    .ToArray());
                var generator = new Compiler.Emit.ReflectionILGenerator(dynamicMethod.GetILGenerator(), false);
                var info = new Compiler.Emit.MethodOptimizationInfo(System.Type.GetType)
                {
                    SyntaxTree = statement,
                    FunctionName = statement.Name,
                    ReturnType = returnType
                };
                statement.GenerateCode(generator, info);
                generator.Complete();
                return dynamicMethod.GetBaseDefinition();
            }
            throw new System.InvalidOperationException("cannot find function");
        }
    }
}
