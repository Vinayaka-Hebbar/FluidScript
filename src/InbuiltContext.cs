using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace FluidScript
{
    internal sealed class InbuiltContext : OperationContext, IReadOnlyOperationContext, IOperationContext, IEnumerable<KeyValuePair<string, Object>>
    {
        internal static readonly IDictionary<string, IdentifierType> identifiers;
        internal static readonly IDictionary<string, IFunction> functions;
        internal static readonly IDictionary<string, Object> mathConstants;

        public IDictionary<string, IdentifierType> Identifiers { get; }

        public override IReadOnlyOperationContext ReadOnlyContext { get; }

        static InbuiltContext()
        {
            identifiers = new Dictionary<string, IdentifierType>
            {
                {"new", IdentifierType.New },
                {"this", IdentifierType.This },
                {"true",IdentifierType.True },
                {"false",IdentifierType.False },
                {"out", IdentifierType.Out },
                {"var", IdentifierType.Var },
                {"function", IdentifierType.Function },
                {"lamda", IdentifierType.Lamda },
                {"if", IdentifierType.If},
                {"else", IdentifierType.Else },
                {"while",IdentifierType.While },
                {"do", IdentifierType.Do },
                {"for", IdentifierType.For },
                {"continue",IdentifierType.Continue },
                {"switch", IdentifierType.Switch },
                {"break", IdentifierType.Break },
                {"throw", IdentifierType.Throw }
            };
            functions = new Dictionary<string, IFunction>();
            mathConstants = new Dictionary<string, Object>();
            #region Initailize
            Add("sin", Sin);
            Add("cos", Cos);
            Add("tan", Tan);
            Add("tanh", Tanh);
            Add("pow", Pow);
            Add("sqrt", Sqrt);
            Add(new Function("iterate", 2, (visitor, args) =>
             {
                 visitor = new NodeVisitor(visitor);
                 var n = args[1].Accept(visitor).ToDouble();
                 var ex = args[0];
                 for (int i = 0; i < n; i++)
                 {
                     ex.Accept(visitor);
                 }
                 return Object.Zero;
             }));
            Add(new Function("log",
                new FunctionPartBuilder(1, (visitor, args) => new Object(Log(args[0].Accept(visitor).ToNumber()))),
                new FunctionPartBuilder(2, (visitor, args) => new Object(Log(args[0].Accept(visitor).ToNumber(), args[1].Accept(visitor).ToNumber())))
                ));
            Add(new Function("round", new FunctionPartBuilder(1, (visitor, args) =>
            {
                return new Object(Round(args[0].Accept(visitor).ToDouble()));
            }),
            new FunctionPartBuilder(2, (visitor, args) => new Object(Round(args[0].Accept(visitor).ToNumber(), (int)args[1].Accept(visitor).ToNumber())))
            ));
            Add("floor", Floor);
            Add("ceiling", Ceiling);
            Add("exp", Exp);
            Add("asin", Asin);
            Add("acos", Acos);
            Add("atan", Atan);
            Add("atan2", Atan2);
            Add(new Function("square", 1, (visitor, args) =>
             {
                 return new Object(Pow(args[0].Accept(visitor).ToNumber(), 2));
             }));
            Add(new Function("print", -1, Print));
            Add(new Function("sum", -1, Sum));
            Add(new Function("average", -1, Average));
            Add(new Function("max", -1, Max));
            Add(new Function("min", -1, Min));
            Add(new Function("parseInt", 1, ParseInt));
            Add(new Function("parseFloat", 1, ParseFloat));
            Add(new Function("parseDouble", 1, ParseDouble));
            //Constants
            mathConstants.Add("pi", new Object(PI, Object.DoubleValueType | ObjectType.Inbuilt));
            mathConstants.Add("e", new Object(E, Object.DoubleValueType | ObjectType.Inbuilt));
            mathConstants.Add("NaN", new Object(double.NaN, Object.DoubleValueType | ObjectType.Inbuilt));
            #endregion
        }

        public InbuiltContext() : base(functions, mathConstants)
        {
            Identifiers = new Dictionary<string, IdentifierType>(identifiers);
            ReadOnlyContext = this;
        }

        public InbuiltContext(IEqualityComparer<string> comparer) : base(functions, mathConstants, comparer)
        {
            Identifiers = new Dictionary<string, IdentifierType>(identifiers, comparer);
            ReadOnlyContext = this;
        }

        #region Static

        internal static void Add(string name, System.Func<double, double> onInvoke)
        {
            functions.Add(name, new Function(name, 1, (visitor, args) => new Object(onInvoke(args[0].Accept(visitor).ToDouble()))));
        }

        internal static void Add(string name, System.Func<double, double, double> onInvoke)
        {
            functions.Add(name, new Function(name, 2, (visitor, args) => new Object(onInvoke(args[0].Accept(visitor).ToDouble(), args[1].Accept(visitor).ToDouble()))));
        }

        public static void Add(IFunction function)
        {
            functions.Add(function.Name, function);
        }

        internal static Object Print(NodeVisitor visitor, IExpression[] expressions)
        {
#if NET35
            System.Console.WriteLine(string.Join(",", expressions.Select(exp => exp.Accept(visitor).ToString()).ToArray()));
#else
            System.Console.WriteLine(string.Join(",", expressions.Select(exp => exp.Accept(visitor))));
#endif
            return Object.Void;
        }

        internal static Object Sum(NodeVisitor visitor, IExpression[] expressions)
        {
            return new Object(expressions.Select(exp => exp.Accept(visitor).ToNumber()).Sum());
        }

        internal static Object Average(NodeVisitor visitor, IExpression[] expressions)
        {
            return new Object(expressions.Select(exp => exp.Accept(visitor).ToNumber()).Average());
        }

        internal static Object Max(NodeVisitor visitor, IExpression[] expressions)
        {
            return new Object(expressions.Select(exp => exp.Accept(visitor).ToNumber()).Max());
        }

        internal static Object Min(NodeVisitor visitor, IExpression[] expressions)
        {
            return new Object(expressions.Select(exp => exp.Accept(visitor).ToNumber()).Min());
        }

        internal static Object ParseInt(NodeVisitor visitor, IExpression[] expressions)
        {
            int.TryParse(expressions.First().Accept(visitor).Raw.ToString(), out int value);
            return new Object(value);
        }

        internal static Object ParseFloat(NodeVisitor visitor, IExpression[] expressions)
        {
            if (expressions.Length == 1)
            {
                float.TryParse(expressions.First().Accept(visitor).Raw.ToString(), out float value);
                return new Object(value);
            }
            return Object.Zero;
        }

        internal static Object ParseDouble(NodeVisitor visitor, IExpression[] expressions)
        {
            if (expressions.Length == 1)
            {
                double.TryParse(expressions.First().Accept(visitor).Raw.ToString(), out double value);
                return new Object(value);
            }
            return Object.Zero;
        }

        #endregion

        public bool TryGetIdentifier(string name, out IdentifierType type)
        {
            return Identifiers.TryGetValue(name, out type);
        }
    }
}
