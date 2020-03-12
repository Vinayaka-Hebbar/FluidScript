using FluidScript.Compiler.SyntaxTree;
namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public abstract class Conversion
    {
        public static readonly Conversion None = new NoConversion();
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

        internal abstract void Generate(Emit.MethodBodyGenerator generator, params SyntaxTree.Expression[] expression);

        internal abstract object Invoke(params object[] args);
    }

    public enum ConversionType
    {
        None,
        Convert,
        ParamArray
    }

    internal sealed class NoConversion : Conversion
    {
        internal NoConversion() : base(-1)
        {
        }

        public override ConversionType ConversionType => ConversionType.None;

        public override System.Type Type => null;

        internal override void Generate(Emit.MethodBodyGenerator generator, params Expression[] expression)
        {
            throw new System.NotImplementedException();
        }

        internal override object Invoke(params object[] args)
        {
            throw new System.NotImplementedException();
        }
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

        internal override void Generate(Emit.MethodBodyGenerator generator, params SyntaxTree.Expression[] expression)
        {
            expression[0].GenerateCode(generator);
            generator.CallStatic(Method);
        }

        internal override object Invoke(params object[] args)
        {
            return Method.Invoke(null, new object[1] { args[0] });
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

        internal override void Generate(Emit.MethodBodyGenerator generator, params SyntaxTree.Expression[] expression)
        {
            // Remaining size
            var size = expression.Length;
            generator.LoadInt32(size);
            generator.NewArray(Type);
            for (int i = 0; i < size; i++)
            {
                generator.Duplicate();
                generator.LoadInt32(i);
                expression[i].GenerateCode(generator);
                if (ParamBinders != null)
                {
                    var binder = ParamBinders.At(i);
                    if (binder != null)
                        binder.Generate(generator, expression[i]);
                }
                generator.StoreArrayElement(Type);
            }
        }

        internal override object Invoke(params object[] args)
        {
            var count = args.Length;
            // Remaining size
            var size = count - Index;
            var newArgs = new object[Index + 1];
            System.Array.Copy(args, newArgs, Index);
            var paramArray = System.Array.CreateInstance(Type, size);
            if (ParamBinders != null)
            {
                foreach (var item in ParamBinders)
                {
                    object value = item.Invoke(args);
                    args[item.Index] = value;
                }
            }
            System.Array.Copy(args, Index, paramArray, 0, size);
            newArgs[Index] = paramArray;
            return newArgs;
        }
    }
}
