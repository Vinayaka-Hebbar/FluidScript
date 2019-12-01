using FluidScript;
using System;
using System.Collections.Generic;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax

            Class1 class1 = new Class1();
            class1.Run();
            Console.ReadKey();
        }

        public void Run()
        {
            var engine = new ScriptEngine();
            var node = engine.ParseFile(System.AppDomain.CurrentDomain.BaseDirectory + "source.fls");
            if (node is FluidScript.Compiler.SyntaxTree.FunctionDeclaration func)
            {
                var method = func.Create();
                var r = method.Invoke(null, new object[0]);
                Console.WriteLine(r);
            }
            if (node is FluidScript.Compiler.SyntaxTree.TypeDeclaration type)
            {
                var dynamicAssembly =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new System.Reflection.AssemblyName("DynamicClass.Utility.DynamicClasses, Version=1.0.0.0"),
                    System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave
                );
                var dynamicModule =
                    dynamicAssembly.DefineDynamicModule("DynamicClass.Utility.DynamicClasses.dll", true);
                var classType = type.Generate(new FluidScript.Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                dynamic instance = Activator.CreateInstance(classType);
                var c = instance.read(1);
                Console.WriteLine(c);
                dynamicAssembly.Save("DynamicClass.Utility.DynamicClasses.dll");

            }
        }

        void Print()
        {
            Queue<int> que = new Queue<int>();
            que.Enqueue(1);
            que.Enqueue(2);
            que.Enqueue(3);
            que.Enqueue(4);
            que.Enqueue(5);
            int count = que.Count;
            for (int i = 0; i < count; i++)
            {
                var item = que.Dequeue();
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }

        public object Test()
        {
            var x = 10;
            return x == 10 ? 1 : 0;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public object Get(object value) => value;
    }

}
