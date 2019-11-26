using FluidScript.Compiler.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LocalFunctionStatement : Statement
    {
        public readonly string Name;

        public readonly ArgumentInfo[] Arguments;

        public readonly Emit.TypeName ReturnType;

        public readonly BlockStatement Body;

        public LocalFunctionStatement(string name, ArgumentInfo[] arguments, TypeName returnType, BlockStatement body) : base(StatementType.Function)
        {
            Name = name;
            Arguments = arguments;
            ReturnType = returnType;
            Body = body;
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", ReturnType.ToString());
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var member = instance.GetPrototype().DeclareMethod(Name, Arguments, ReturnType, Body);
            var reference = new Core.DynamicFunction(member, instance, member.DynamicInvoke);
            instance[Name] = reference;
            return reference;
        }

        internal override RuntimeObject Evaluate(RuntimeObject instance, Metadata.Prototype prototype)
        {
            var member = prototype.DeclareMethod(Name, Arguments, ReturnType, Body);
            var reference = new Core.DynamicFunction(member, instance, member.DynamicInvoke);
            instance[Name] = reference;
            return reference;
        }
#endif

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            Body.GenerateCode(generator, info);
            if (info.ReturnTarget != null)
                generator.DefineLabelPosition(info.ReturnTarget);
            if (info.ReturnVariable != null)
                generator.LoadVariable(info.ReturnVariable);
        }
    }
}
