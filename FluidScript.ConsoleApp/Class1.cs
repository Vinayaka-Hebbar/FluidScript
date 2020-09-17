using FluidScript.Collections;
using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
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

        public void Test()
        {
            try
            {
                //Runtime();
                // FluidTest.Sample sample = new FluidTest.Sample();
                // var res=  sample.Create();
                //FuncTest();
                CodeGen();
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        private static void Runtime()
        {
            RuntimeCompiler compiler = new RuntimeCompiler();
            compiler.Locals["a"] = 10;
            compiler.Locals["b"] = null;
            var res = (Delegate)compiler.Invoke(Parser.GetStatement(
                @"{
                 import {console=System.Console} from mscorlib;
                console.Write('Value {0}', 2);
                }"), new object());
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

        public void InstanceOf(object x)
        {

            List<Any> values = new List<Any>();
            values.AddRange(new Any[]
            {
                new Any(new Integer(0)),
                new Any(new Integer(1))
            });
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
            Action x = () => Console.WriteLine(i);
            i = 10;
            return x;
        }
    }
}
