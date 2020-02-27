namespace FluidScript.Compiler.Binders
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public abstract class ArgumentBinder
    {
        public readonly int Index;

        public abstract ArgumentBindType BindType { get; }

        /// <summary>
        /// Initializes new <see cref="ArgumentBinder"/>
        /// </summary>
        public ArgumentBinder(int index)
        {
            Index = index;
        }

        internal virtual void Generate(Emit.MethodBodyGenerator generator)
        {
        }

        internal abstract object Invoke(System.Collections.IList args);

        public enum ArgumentBindType
        {
            Convert,
            ParamArray
        }
    }

    internal sealed class ParamConvert : ArgumentBinder
    {
        /// <summary>
        /// Conversion method
        /// </summary>
        public readonly System.Reflection.MethodInfo Method;

        public ParamConvert(int index, System.Reflection.MethodInfo method) : base(index)
        {
            Method = method;
        }

        public override ArgumentBindType BindType => ArgumentBindType.Convert;

        internal override void Generate(Emit.MethodBodyGenerator generator)
        {
            generator.CallStatic(Method);
        }

        internal override object Invoke(System.Collections.IList args)
        {
            return Method.Invoke(null, new object[1] { args[Index] });
        }
    }

    internal sealed class ParamArrayBinder : ArgumentBinder
    {
        public readonly System.Type Type;

        public ParamArrayBinder(int index, System.Type type) : base(index)
        {
            Type = type;
        }

        public override ArgumentBindType BindType => ArgumentBindType.ParamArray;

        internal override void Generate(Emit.MethodBodyGenerator generator)
        {
            //Not Implemented
            throw new System.NotImplementedException();
        }

        internal override object Invoke(System.Collections.IList args)
        {
            var count = args.Count;
            // 7 - 4 = 3
            var size = count - Index;
            var newArgs = new object[Index + 1];
            for (int index = 0; index < Index; index++)
            {
                object item = args[index];
                newArgs[index] = item;
            }
            var paramArray = System.Array.CreateInstance(Type.GetElementType(), size);
            args.CopyTo(paramArray, Index);
            newArgs[Index] = paramArray;
            return newArgs;
        }
    }
}
