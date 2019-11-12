using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;

namespace FluidScript.Library
{
    public sealed class MathObject : RuntimeObject
    {
        [Callable("pow", ArgumentTypes.Double, ArgumentTypes.Double)]
        public static RuntimeObject Pow(RuntimeObject arg1, RuntimeObject arg2)
        {
            return System.Math.Pow(arg1.ToDouble(), arg2.ToDouble());
        }

        [Callable("sqrt", ArgumentTypes.Double)]
        public static RuntimeObject Sqrt(RuntimeObject arg1)
        {
            return System.Math.Sqrt(arg1.ToDouble());
        }

        [Callable("log", ArgumentTypes.Double)]
        public static RuntimeObject Log(RuntimeObject arg1)
        {
            return System.Math.Log(arg1.ToDouble());
        }

        [Callable("sin", ArgumentTypes.Double)]
        public static RuntimeObject Sin(RuntimeObject arg1)
        {
            return System.Math.Sin(arg1.ToDouble());
        }

        [Callable("cos", ArgumentTypes.Double)]
        public static RuntimeObject Cos(RuntimeObject arg1)
        {
            return System.Math.Cos(arg1.ToDouble());
        }

        [Callable("tan", ArgumentTypes.Double)]
        public static RuntimeObject Tan(RuntimeObject arg1)
        {
            return System.Math.Tan(arg1.ToDouble());
        }

        [Callable("atan", ArgumentTypes.Double)]
        public static RuntimeObject ATan(RuntimeObject arg1)
        {
            return System.Math.Atan(arg1.ToDouble());
        }

        [Callable("atan2", ArgumentTypes.Double)]
        public static RuntimeObject ATan2(RuntimeObject arg1, RuntimeObject arg2)
        {
            return System.Math.Atan2(arg1.ToDouble(), arg2.ToDouble());
        }

        [Callable("round", ArgumentTypes.Double)]
        public static RuntimeObject Round(RuntimeObject arg1)
        {
            return System.Math.Round(arg1.ToDouble());
        }

        [Callable("sum", ArgumentTypes.VarArg)]
        public static RuntimeObject Sum(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (Core.ArrayObject)arg;
                    return System.Linq.Enumerable.Sum(array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Sum(args, selector);
        }

        [Callable("avg", ArgumentTypes.VarArg)]
        public static RuntimeObject Avg(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (Core.ArrayObject)arg;
                    return System.Linq.Enumerable.Average(array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Average(args, selector);
        }

        [Callable("max", ArgumentTypes.VarArg)]
        public static RuntimeObject Max(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (Core.ArrayObject)arg;
                    return System.Linq.Enumerable.Max(array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Max(args, selector);
        }

        [Callable("min", ArgumentTypes.VarArg)]
        public static RuntimeObject Min(RuntimeObject[] args)
        {
            double selector(RuntimeObject arg)
            {
                if (arg.IsArray())
                {
                    var array = (Core.ArrayObject)arg;
                    return System.Linq.Enumerable.Min(array, selector);
                }
                return arg.ToNumber();
            }
            return System.Linq.Enumerable.Min(args, selector);
        }

        [Callable("abs", ArgumentTypes.Double)]
        public static RuntimeObject Abs(RuntimeObject arg1)
        {
            return System.Math.Abs(arg1.ToDouble());
        }

        [Callable("floor", ArgumentTypes.Double)]
        public static RuntimeObject Floor(RuntimeObject arg1)
        {
            return System.Math.Floor(arg1.ToDouble());
        }

        [Callable("ceiling", ArgumentTypes.Double)]
        public static RuntimeObject Ceiling(RuntimeObject arg1)
        {
            return System.Math.Ceiling(arg1.ToDouble());
        }

    }
}
