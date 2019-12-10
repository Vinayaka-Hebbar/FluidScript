using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NameExpression : Expression
    {
        public readonly string Name;

        public Binding Binding { get; internal set; }

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
            var field = Enumerable.FirstOrDefault(Enumerable.OfType<Reflection.DeclaredField>(prototype.GetMembers()), m => m.Name == Name);
            if (field != null)
                return field.DefaultValue ?? RuntimeObject.Null;
            return RuntimeObject.Null;
        }
#endif

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (Binding.IsMember && generator.Method.IsStatic == false)
                generator.LoadArgument(0);
            Binding.GenerateGet(generator);
        }

    }
}
