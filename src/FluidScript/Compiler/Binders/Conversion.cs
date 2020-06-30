using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public abstract class Conversion
    {
        public readonly int Index;

        public abstract ConversionType ConversionType { get; }

        /// <summary>
        /// case when Int32 -> FluidScript.Integer and FluidScript.Double to double ex: int32 * FluidScript.Double
        /// </summary>
        internal Conversion next;

        public abstract Type Type { get; }

        /// <summary>
        /// Initializes new <see cref="Conversion"/>
        /// </summary>
        public Conversion(int index)
        {
            Index = index;
        }

        public void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            Conversion n = this;
            do
            {
                n = n.next;
                n.InternalGenerateCode(generator);
            } while (n != this);
        }

        protected virtual void InternalGenerateCode(Emit.MethodBodyGenerator generator)
        {
            throw new NotImplementedException(nameof(GenerateCode));
        }

        public virtual void GenerateCode(Emit.MethodBodyGenerator generator, Expression[] expressions)
        {
            throw new NotImplementedException(nameof(GenerateCode));
        }

        internal abstract object Invoke(params object[] args);
    }

    public enum ConversionType
    {
        Normal,
        ParamArray,
    }

    internal sealed class BoxConversion : Conversion
    {
        public BoxConversion(int index, Type type) : base(index)
        {
            Type = type;
        }

        public override ConversionType ConversionType => ConversionType.Normal;

        public override Type Type { get; }

        internal override object Invoke(params object[] args)
        {
            return args[0];
        }

        protected override void InternalGenerateCode(Emit.MethodBodyGenerator generator)
        {
            generator.Box(Type);
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

        public override Type Type => Method.ReturnType;

        public override ConversionType ConversionType => ConversionType.Normal;

        protected override void InternalGenerateCode(Emit.MethodBodyGenerator generator)
        {
            generator.CallStatic(Method);
        }

        internal override object Invoke(params object[] args)
        {
            return Method.Invoke(null, new object[1] { args[0] });
        }

        public override string ToString()
        {
            return string.Concat(nameof(ConversionType.Normal), "(", Type, ")");
        }
    }

    internal sealed class ParamArrayConversion : Conversion
    {
        public override Type Type { get; }

        public ArgumentConversions ParamBinders { get; }

        public ParamArrayConversion(int index, Type type) : base(index)
        {
            Type = type;
        }

        public ParamArrayConversion(int index, Type type, ArgumentConversions paramBinders) : base(index)
        {
            Type = type;
            ParamBinders = paramBinders;
        }

        public override ConversionType ConversionType => ConversionType.ParamArray;

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Expression[] expression)
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
            Array.Copy(args, newArgs, Index);
            Array.Copy(args, Index, paramArray, 0, size);
            newArgs[Index] = paramArray;
            return newArgs;
        }
    }
}
