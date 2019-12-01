using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LocalFunctionStatement : Statement
    {
        public readonly string Name;

        public readonly TypeParameter[] Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public LocalFunctionStatement(string name, TypeParameter[] arguments, TypeSyntax returnType, BlockStatement body) : base(StatementType.Function)
        {
            Name = name;
            Parameters = arguments;
            ReturnType = returnType;
            Body = body;
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType.ToString());
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var member = instance.GetPrototype().DeclareMethod(Name, Parameters.Select(arg => arg.GetParameterInfo()).ToArray(), ReturnType.GetTypeInfo(), Body);
            var reference = new Core.DynamicFunction(member, instance, member.DynamicInvoke);
            instance[Name] = reference;
            return reference;
        }

        internal override RuntimeObject Evaluate(RuntimeObject instance, Metadata.Prototype prototype)
        {
            var member = prototype.DeclareMethod(Name, Parameters.Select(arg=>arg.GetParameterInfo()).ToArray(), ReturnType.GetTypeInfo(), Body);
            var reference = new Core.DynamicFunction(member, instance, member.DynamicInvoke);
            instance[Name] = reference;
            return reference;
        }
#endif

        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            Body.GenerateCode(generator);
            if (generator.ReturnTarget != null)
                generator.DefineLabelPosition(generator.ReturnTarget);
            if (generator.ReturnVariable != null)
                generator.LoadVariable(generator.ReturnVariable);
        }
    }
}
