using FluidScript.Collections;
using FluidScript.Compiler;
using FluidScript.Compiler.Binders;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using System;
using System.Reflection;

namespace FluidScript.ConsoleApp
{
    public class Class1
    {
        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        Func<Any, Any> value = (s) => s;

        private struct Resolver : IMemberResolver, IMemberBinder
        {
            public Type Type => typeof(int);

            public object Get(object obj)
            {
                return 10;
            }

            public IMemberBinder Resolve(NameExpression node)
            {
                return this;
            }

            public MethodInfo Resolve(InvocationExpression node, string name, object obj, object[] args)
            {
                throw new NotImplementedException();
            }

            public IMemberBinder Resolve(MemberExpression node)
            {
                return MemberBinder.Empty;
            }

            public void Set(object obj, object value)
            {
                throw new NotImplementedException();
            }
        }

        void Compile()
        {
            Type x = typeof(int);
            if(x is null)
            Console.Write(x);
            if(x == null)
                Console.WriteLine(x);
        }

        public void Test()
        {
            try
            {
                var type = typeof(ValueType);
                Runtime();

                // FluidTest.Sample sample = new FluidTest.Sample();
                // var res=  sample.Create();
                //FuncTest();
                // RunCodeGen();
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        private void Runtime()
        {
            RuntimeCompiler compiler = new RuntimeCompiler();
            var target = new DynamicObject();
            compiler["narrowPitchCoils"] = new Double(1);
            var statement = ScriptParser.ParseText(@"{
        b=2+3;
        return b;
}");
            var res = compiler.Invoke(Expression.Empty);
            Console.WriteLine(res);
        }

        static void FuncTest()
        {
            var compiler = new RuntimeCompiler();
            var res = compiler.Invoke((Statement)ScriptParser.ParseText("(7.46-13.2)/1320"));
            compiler.Locals["deflns"] = new DynamicObject
            {
                ["solidHt"] = 172.5,
                ["len2"] = 120.0,
                ["endur"] = 118.11,
                ["cOP1"] = 85.0,
                ["len1"] = 20.0,
                ["preload"] = 14.11
            };
            compiler.Locals["loads"] = new DynamicObject
            {
                ["solidHt"] = new DynamicObject
                {
                    ["v1"] = 10,
                    ["v2"] = 20
                },
                ["len2"] = new DynamicObject
                {
                    ["v1"] = 10,
                    ["v2"] = 20
                },
            };
            System.Collections.Generic.IDictionary<string, object> crted = new DynamicObject
            {
                ["endur"] = new DynamicObject
                {
                    ["v1"] = 841.45,
                    ["v2"] = 20
                },
                ["preload"] = new DynamicObject
                {
                    ["v1"] = 78.54,
                    ["v2"] = 20
                },
            };
            compiler.Locals["crted"] = crted;
            compiler.Locals["points"] = new List<String>
            {
                "preload",
                "len1",
                "cOP1",
                "endur",
                "len2",
                "solidHt",
            };
            compiler.Locals["springRate1"] = 5.89;
            compiler.Locals["springRate2"] = 8.89;
            compiler.Locals["pointsLen"] = 6;
            compiler.Locals["meanDia"] = 32;
            compiler.Locals["barDia"] = 5;
            compiler.Locals["pi"] = 3.14;
            var node = Parser.GetStatement("this.preload.v1");
            object value = compiler.Invoke(node, crted);
            Console.WriteLine(value);
        }

        static void RunCodeGen()
        {
            var code = ScriptParser.ParseProgram("source.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            code.Compile(assembly);
            assembly.Save("FluidTest.dll");
            if (assembly.Context.TryGetType("Sample", out Type type))
            {
                if (type is IType)
                {
                    type = type.ReflectedType;
                }
                object obj = Activator.CreateInstance(type);
                Any value = new Any(obj);
                var res = value.Invoke("create");
                Console.WriteLine(res);
            }
        }

        private static void CodeGen()
        {
            //Integer x = 0;
            var code = ScriptParser.ParseProgram("source.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            code.Compile(assembly);
            assembly.Save("FluidTest.dll");
        }

        DynamicObject x = new DynamicObject()
        {
            ["a"] = 1,
            ["b"] = 2,
        };

        protected bool IsNull(object x)
        {
            return x is null;
        }

        public void InstanceOf()
        {
            var x = this.x;
            x["a"] = 10;
        }

        public override string ToString()
        {
            object value = 10;
            return value.ToString();
        }

        int i;
        public Action TestFun3()
        {
            i = Console.Read();
            void x() => Console.WriteLine(i);
            i = 10;
            return x;
        }
    }
}
