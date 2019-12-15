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
            class1.EmitRun();
            //class1.Print();
            Console.ReadKey();
        }

        private void Run()
        {
            ScriptEngine engine = new ScriptEngine();
            var context = new FluidScript.Dynamic.DynamicContext(new FluidScript.Math());
            context["a"] = new FluidScript.String("");
            context["b"] = new Integer(20);
            FluidScript.Compiler.SyntaxTree.Statement tree = engine.GetStatement("pow(2,min([4,5]))");
            var re = context.Invoke(tree);
            Console.WriteLine(re);
        }

        void Test()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine();
                if (i == 1)
                    continue;
                break;
            }
        }

        void Test1()
        {
            var i = 0;
            do
            {
                Console.WriteLine();
                i++;
            } while (i < 10);
        }

        void Test2()
        {
            List<int> a = new List<int>();
            a[0] = 10;
        }

        void EmitRun()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.ParseFile(path + "source.fls");
            if (tree is FluidScript.Compiler.SyntaxTree.TypeDeclaration declration)
            {
                System.Reflection.AssemblyName aName = new System.Reflection.AssemblyName("Runtime.DynamicClasses, Version=1.0.0.1");
                var dynamicAssembly =
               System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(aName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);

                var dynamicModule =
                    dynamicAssembly.DefineDynamicModule(aName.Name, string.Concat(aName.Name, ".dll"), false);
                var classType = declration.Generate(new FluidScript.Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                dynamic instance = Activator.CreateInstance(classType);
                dynamicAssembly.Save("Runtime.DynamicClasses.dll");
                object result = instance.Add();
                Console.WriteLine(result);
            }
        }
    }

}
