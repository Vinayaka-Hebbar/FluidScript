using NUnit.Framework;
using System;
using System.Diagnostics;

namespace FluidScript.Tests
{
    public class Tests
    {
        object instance;

        [SetUp]
        public void Setup()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.ParseFile(path + "source.fls");
            if (tree is Compiler.SyntaxTree.TypeDeclaration declration)
            {
                System.Reflection.AssemblyName aName = new System.Reflection.AssemblyName("Runtime.DynamicClasses, Version=1.0.0.1");
                var dynamicAssembly =
               System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(aName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndCollect);

                var dynamicModule =
                    dynamicAssembly.DefineDynamicModule(aName.Name);
                var classType = declration.Generate(new Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                instance = Activator.CreateInstance(classType);
            }
        }

        [Test]
        public void Create_Class_Add()
        {
            dynamic value = instance;
            object result = value.Add();
            Debug.Write(result);
            Assert.AreEqual(result, new Integer(18));
        }
    }
}