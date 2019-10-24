using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Compiler.Reflection
{
    public class MethodBody
    {
        public readonly MethodBase DeclaredMethod;
        private IList<LocalVariableInfo> localVariables;
        private BlockStatement syntaxTree;

        public MethodBody(MethodBase declaredMethod)
        {
            DeclaredMethod = declaredMethod;
        }

        public LocalVariableInfo DeclareLocalVariable(string name, TypeInfo type)
        {
            if (localVariables == null)
                localVariables = new List<LocalVariableInfo>();
            var localVariable = new LocalVariableInfo(name, type, localVariables.Count);
            localVariables.Add(localVariable);
            return localVariable;
        }

        public void Generate(System.Reflection.Emit.MethodBuilder builder)
        {
            if (syntaxTree == null)
                return;
            var info = new Emit.OptimizationInfo();
            var generator = new Emit.ReflectionILGenerator(builder.GetILGenerator(), false);
            foreach (var statement in syntaxTree)
            {
                statement.GenerateCode(generator, info);
            }
        }
    }
}
