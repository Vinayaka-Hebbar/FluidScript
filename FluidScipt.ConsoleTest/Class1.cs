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
            class1.Print();
            Console.ReadKey();
        }

        private void Print()
        {
        }

        private void OnHubRecieve(object obj)
        {
            Console.WriteLine(obj);
        }

        private void Run()
        {
            var context = new FluidScript.Dynamic.DynamicContext(new FluidScript.Math());
            context["a"] = new FluidScript.Double(4);
            FluidScript.Compiler.SyntaxTree.Statement tree = ScriptEngine.GetStatement("{return null;}", FluidScript.Compiler.ParserSettings.Default);
            var re = context.Invoke(tree);
            FluidScript.Compiler.SyntaxTree.Statement tree1 = ScriptEngine.GetStatement("{a=2;}", FluidScript.Compiler.ParserSettings.Default);
            var re1 = context.Invoke(tree1);
            Console.WriteLine(re);
        }

        void EmitRun()
        {
            var type = typeof(Dictionary<string, double>);
            Console.WriteLine(type.IsSerializable);
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


        public class Test
        {
            public int A { get; }
            [System.Runtime.Serialization.DataMember]
            public int B { get; }
        }
    }

}
