using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Generators;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using System;
using System.Diagnostics;
using System.Reflection;
using ParameterInfo = FluidScript.Compiler.Emit.ParameterInfo;

namespace FluidScript.ConsoleApp
{
    public class AnyClass
    {
        readonly TypeGenerator typeGen;
        readonly AssemblyGen assembly;

        FieldGenerator valueField;

        internal AnyClass()
        {
            assembly = new AssemblyGen("FluidScript.Runtime", "1.0");
            typeGen = assembly.DefineType("Any", typeof(ValueType), TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit);
        }
        internal static void Run()
        {
            AnyClass any = new AnyClass();
            any.Start();
        }

        internal void Start()
        {
            assembly.Context.Register("Console", typeof(Console));
            // code.Compile(assembly);
            valueField = typeGen.DefineField("value", typeof(object), FieldAttributes.Private);
            valueField.SetCustomAttribute(typeof(DebuggerBrowsableAttribute), typeof(DebuggerBrowsableAttribute).GetInstanceCtor(typeof(DebuggerBrowsableState)), DebuggerBrowsableState.Never);
            // ctor
            var ctorParams = new ParameterInfo[] { new ParameterInfo("val", 0, typeof(object)) };
            ConstructorGenerator ctorGen = typeGen.DefineCtor(ctorParams, MethodAttributes.Public);
            ctorGen.SyntaxBody = new BlockStatement(new NodeList<Statement>
                {
                    Expression.Assign(Expression.Member("value"), Expression.Parameter(ctorParams[0]))
                });

            EmitOpImplicit(typeGen);
            EmitOpAddition(typeGen);
            EmitToStringMethod();
            typeGen.CreateType();
            assembly.Save("FluidScript.Runtime.dll");
        }

        private void EmitToStringMethod()
        {
            var toStringMethod = typeGen.DefineMethod("ToString", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, new ParameterInfo[0], typeof(string));
            toStringMethod.SyntaxBody = new BlockStatement(new NodeList<Statement>
                {
                    Statement.If(Expression.IsInstanceOf(Expression.Member(valueField), null), Statement.Return(Expression.SystemLiteral(string.Empty))),
                    Statement.Return(Expression.Call(Expression.Member(valueField), typeof(object).GetInstanceMethod("ToString")))
                });
        }

        private static void EmitOpImplicit(TypeGenerator typeGen)
        {
            ParameterInfo[] methodParams = new ParameterInfo[] { new ParameterInfo("val", 0, typeof(object)) };
            var methodGen = typeGen.DefineMethod("op_Implicit", MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public, methodParams, typeGen);

            methodGen.SyntaxBody = new BlockStatement(new NodeList<Statement>
                {
                    Statement.Return(Expression.New(typeGen, Expression.Parameter(methodParams[0])))
                });
        }

        private static void EmitOpAddition(TypeGenerator typeGen)
        {
            ParameterInfo[] methodParams = new ParameterInfo[] { new ParameterInfo("left", 0, typeGen), new ParameterInfo("right", 1, typeGen) };
            var methodGen = typeGen.DefineMethod("op_Addition", MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.Public, methodParams, typeGen);

            methodGen.SyntaxBody = new BlockStatement(new NodeList<Statement>
                {
                    Statement.Return(Expression.Custom((e, g)=> {
                        e.Type = typeof(int);
                        g.LoadInt32(10);
                    }))
                });
        }
    }
}
