using FluidScript;
using System;
using System.Runtime.Serialization;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax

            Class1 class1 = new Class1();
            // class1.Run();
            class1.Print();
            Console.ReadKey();
        }

        private void Run()
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
                    dynamicAssembly.DefineDynamicModule(aName.Name, string.Concat(aName.Name, ".dll"), true);
                var classType = declration.Generate(new FluidScript.Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                dynamic instance = Activator.CreateInstance(classType);
                var c = instance.Read(1);
                Console.WriteLine(c);
                dynamicAssembly.Save("Runtime.DynamicClasses.dll");
            }
        }

        void Print()
        {
            dynamic value = new System.Dynamic.ExpandoObject();
            value.a = new int[] { 1 };
           var r = value.a[0];
            Runtime.DynamicClasses.Test test = new Runtime.DynamicClasses.Test();
            Console.WriteLine();
        }
    }

}
