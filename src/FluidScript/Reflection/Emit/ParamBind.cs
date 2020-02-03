namespace FluidScript.Reflection.Emit
{
    /// <summary>
    /// Operator overload conversion
    /// </summary>
    public class ParamBind
    {
        internal static readonly ParamBind Empty = new ParamBind(-1);


        public readonly int Index;

        public virtual ParamBindType BindType => ParamBindType.None;

        /// <summary>
        /// Initializes new <see cref="ParamBind"/>
        /// </summary>
        public ParamBind(int index)
        {
            Index = index;
        }

        internal virtual void Generate(MethodBodyGenerator generator)
        {

        }

        internal virtual object Invoke(System.Collections.IList args)
        {
            return null;
        }

        public enum ParamBindType
        {
            None,
            Convert,
            ParamArray
        }
    }

    internal sealed class ParamConvert : ParamBind
    {
        /// <summary>
        /// Conversion method
        /// </summary>
        public readonly System.Reflection.MethodInfo Method;

        public ParamConvert(int index, System.Reflection.MethodInfo method) : base(index)
        {
            Method = method;
        }

        public override ParamBindType BindType => ParamBindType.Convert;

        internal override void Generate(MethodBodyGenerator generator)
        {
            generator.CallStatic(Method);
        }

        internal override object Invoke(System.Collections.IList args)
        {
            return Method.Invoke(null, new object[1] { args[Index] });
        }
    }

    internal sealed class ParamArrayBind : ParamBind
    {
        public readonly System.Type Type;

        public ParamArrayBind(int index, System.Type type) : base(index)
        {
            Type = type;
        }

        public override ParamBindType BindType => ParamBindType.ParamArray;

        internal override void Generate(MethodBodyGenerator generator)
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
