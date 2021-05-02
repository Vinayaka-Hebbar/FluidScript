using System;

namespace FluidScript.Runtime
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

        public Conversion Next => next;

        public abstract object Invoke(params object[] args);
    }

    public enum ConversionType
    {
        Normal,
        ParamArray,
    }

    public sealed class BoxConversion : Conversion
    {
        public BoxConversion(int index, Type type) : base(index)
        {
            Type = type;
        }

        public override ConversionType ConversionType => ConversionType.Normal;

        public override Type Type { get; }

        public override object Invoke(params object[] args)
        {
            return args[0];
        }
    }

    public sealed class ParamConversion : Conversion
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

        public override object Invoke(params object[] args)
        {
            return Method.Invoke(null, new object[1] { args[0] });
        }

        public Conversion Append(Conversion conversion)
        {
            conversion.next = this;
            next = conversion;
            return conversion;
        }

        public override string ToString()
        {
            return string.Concat("Convert(", Type, ")");
        }
    }

    public sealed class ParamArrayConversion : Conversion
    {
        public override Type Type { get; }

        public ArgumentConversions Conversions { get; }

        public ParamArrayConversion(int index, Type type) : base(index)
        {
            Type = type;
        }

        public ParamArrayConversion(int index, Type type, ArgumentConversions conversions) : base(index)
        {
            Type = type;
            Conversions = conversions;
        }

        public override ConversionType ConversionType => ConversionType.ParamArray;

        public override object Invoke(params object[] args)
        {
            var count = args.Length;
            // Remaining size
            var size = count - Index;
            var newArgs = new object[Index + 1];
            var paramArray = Array.CreateInstance(Type, size);
            if (Conversions != null)
            {
                Conversions.Invoke(ref args);
            }
            Array.Copy(args, newArgs, Index);
            Array.Copy(args, Index, paramArray, 0, size);
            newArgs[Index] = paramArray;
            return newArgs;
        }
    }
}
