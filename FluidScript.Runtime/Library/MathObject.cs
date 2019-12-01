using FluidScript.Reflection.Emit;
using FluidScript.Reflection;

namespace FluidScript.Library
{
    public sealed class MathObject : RuntimeObject
    {
        [Field("pi", RuntimeType.Double)]
        public static readonly RuntimeObject PI = System.Math.PI;

        [Callable("pow", RuntimeType.Double, ArgumentTypes.Double, ArgumentTypes.Double)]
        public static RuntimeObject Pow(RuntimeObject arg1, RuntimeObject arg2)
        {
            return System.Math.Pow(arg1.ToDouble(), arg2.ToDouble());
        }

        [Callable("sqrt", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Sqrt(RuntimeObject arg1)
        {
            return System.Math.Sqrt(arg1.ToDouble());
        }

        [Callable("log", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Log(RuntimeObject arg1)
        {
            return System.Math.Log(arg1.ToDouble());
        }

        [Callable("sin", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Sin(RuntimeObject arg1)
        {
            return System.Math.Sin(arg1.ToDouble());
        }

        [Callable("cos", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Cos(RuntimeObject arg1)
        {
            return System.Math.Cos(arg1.ToDouble());
        }

        [Callable("tan", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Tan(RuntimeObject arg1)
        {
            return System.Math.Tan(arg1.ToDouble());
        }

        [Callable("atan", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject ATan(RuntimeObject arg1)
        {
            return System.Math.Atan(arg1.ToDouble());
        }

        [Callable("atan2", RuntimeType.Double, ArgumentTypes.Double, ArgumentTypes.Double)]
        public static RuntimeObject ATan2(RuntimeObject arg1, RuntimeObject arg2)
        {
            return System.Math.Atan2(arg1.ToDouble(), arg2.ToDouble());
        }

        [Callable("round", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Round(RuntimeObject arg1)
        {
            return System.Math.Round(arg1.ToDouble());
        }

        [Callable("sum", RuntimeType.Double, ArgumentTypes.VarArg)]
        public static RuntimeObject Sum(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (ArrayObject)arg;
                    return System.Linq.Enumerable.Sum((System.Collections.Generic.IEnumerable<RuntimeObject>)array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Sum(args, selector);
        }

        [Callable("avg", RuntimeType.Double, ArgumentTypes.VarArg)]
        public static RuntimeObject Avg(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (ArrayObject)arg;
                    if (array.Length > 0)
                        return System.Linq.Enumerable.Average((System.Collections.Generic.IEnumerable<RuntimeObject>)array, selector);
                    return 0;
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Average(args, selector);
        }

        [Callable("max", RuntimeType.Double, ArgumentTypes.VarArg)]
        public static RuntimeObject Max(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (ArrayObject)arg;
                    return System.Linq.Enumerable.Max((System.Collections.Generic.IEnumerable<RuntimeObject>)array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Max(args, selector);
        }

        [Callable("min", RuntimeType.Double, ArgumentTypes.VarArg)]
        public static RuntimeObject Min(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (ArrayObject)arg;
                    return System.Linq.Enumerable.Min((System.Collections.Generic.IEnumerable<RuntimeObject>)array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Min(args, selector);
        }

        [Callable("abs", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Abs(RuntimeObject arg1)
        {
            return System.Math.Abs(arg1.ToDouble());
        }

        [Callable("floor", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Floor(RuntimeObject arg1)
        {
            return System.Math.Floor(arg1.ToDouble());
        }

        [Callable("ceiling", RuntimeType.Double, ArgumentTypes.Double)]
        public static RuntimeObject Ceiling(RuntimeObject arg1)
        {
            return System.Math.Ceiling(arg1.ToDouble());
        }

        public override string ToString()
        {
            return "System.Math";
        }
    }
}
