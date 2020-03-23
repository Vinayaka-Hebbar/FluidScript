using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public abstract class Conversion
    {
        public readonly int Index;

        public abstract ConversionType ConversionType { get; }

        public abstract System.Type Type { get; }

        /// <summary>
        /// Initializes new <see cref="Conversion"/>
        /// </summary>
        public Conversion(int index)
        {
            Index = index;
        }

        internal virtual void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            throw new System.NotImplementedException(nameof(GenerateCode));
        }

        internal virtual void GenerateCode(Emit.MethodBodyGenerator generator, Expression[] expressions)
        {
            throw new System.NotImplementedException(nameof(GenerateCode));
        }

        internal abstract object Invoke(params object[] args);
    }

    public enum ConversionType
    {
        None,
        Convert,
        ParamArray
    }

    internal sealed class ParamConversion : Conversion
    {
        /// <summary>
        /// Conversion method
        /// </summary>
        public readonly System.Reflection.MethodInfo Method;

        public ParamConversion(int index, System.Reflection.MethodInfo method) : base(index)
        {
            Method = method;
        }

        public override System.Type Type => Method.ReturnType;

        public override ConversionType ConversionType => ConversionType.Convert;

        internal override void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            generator.CallStatic(Method);
        }

        internal override object Invoke(params object[] args)
        {
            return Method.Invoke(null, new object[1] { args[0] });
        }

        public override string ToString()
        {
            return string.Concat(nameof(ConversionType.Convert), "(", Type, ")");
        }
    }

    internal sealed class ParamArrayConversion : Conversion
    {
        public override System.Type Type { get; }

        public ArgumentConversions ParamBinders { get; }

        public ParamArrayConversion(int index, System.Type type) : base(index)
        {
            Type = type;
        }

        public ParamArrayConversion(int index, System.Type type, ArgumentConversions paramBinders) : base(index)
        {
            Type = type;
            ParamBinders = paramBinders;
        }

        public override ConversionType ConversionType => ConversionType.ParamArray;

        internal override void GenerateCode(Emit.MethodBodyGenerator generator, Expression[] expression)
        {
            // Remaining size
            var size = expression.Length;
            generator.LoadInt32(size);
            generator.NewArray(Type);
            if (size > 0)
            {
                var conversions = ParamBinders;
                for (int i = 0; i < size; i++)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i);
                    Expression exp = expression[i];
                    exp.GenerateCode(generator);
                    if (exp.Type.IsValueType && Type.IsValueType == false)
                        generator.Box(exp.Type);
                    if (conversions != null)
                    {
                        var group = conversions[i];
                        if (group != null)
                            group.GenerateCode(generator);
                    }
                    generator.StoreArrayElement(Type);
                }
            }
        }

        internal override object Invoke(params object[] args)
        {
            var count = args.Length;
            // Remaining size
            var size = count - Index;
            var newArgs = new object[Index + 1];
            var paramArray = System.Array.CreateInstance(Type, size);
            if (ParamBinders != null)
            {
                ParamBinders.Invoke(ref args);
            }
            System.Array.Copy(args, newArgs, Index);
            System.Array.Copy(args, Index, paramArray, 0, size);
            newArgs[Index] = paramArray;
            return newArgs;
        }
    }
}
