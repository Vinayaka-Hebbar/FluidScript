using System;
using System.Collections.Generic;

namespace FluidScript.Compiler.Metadata
{
    /// <summary>
    /// Prototype of current method or class
    /// </summary>
    public abstract class Prototype
    {
        public readonly Prototype Parent;
        public readonly ScopeContext Context;
        public readonly string Name;

#if Runtime
        internal static Prototype prototype;
        internal static Prototype Default
        {
            get
            {
                if (prototype == null)
                {
                    var type = typeof(RuntimeObject);
                    var methods = Reflection.MemberInvoker.GetMethods(type);
                    prototype = new DefaultObjectPrototype(methods);
                }
                return prototype;
            }
        }

        internal static ObjectPrototype Create(Type type)
        {
            var constants = System.Linq.Enumerable.Empty<KeyValuePair<string, RuntimeObject>>();
            var variables = System.Linq.Enumerable.Empty<KeyValuePair<string, Reflection.DeclaredVariable>>();
            var methods = Reflection.MemberInvoker.GetMethods(type);
            return new ObjectPrototype(constants, variables, methods);
        }
#endif

        public Prototype(Prototype parent, string name, ScopeContext context)
        {
            Parent = parent;
            Name = name;
            Context = context;
        }

        internal virtual Reflection.DeclaredMethod DeclareMethod(SyntaxTree.FunctionDeclaration declaration, SyntaxTree.BodyStatement body)
        {
            throw new System.Exception("Can't declare method inside " + GetType());
        }

        internal virtual Reflection.DeclaredVariable DeclareLocalVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare local variable inside " + GetType());
        }

        internal virtual Reflection.DeclaredVariable DeclareVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare variable inside " + GetType());
        }

        public abstract IEnumerable<KeyValuePair<string, Reflection.DeclaredVariable>> GetVariables();

        public abstract IEnumerable<Reflection.DeclaredMethod> GetMethods();

        public abstract bool HasVariable(string name);

#if Runtime
        public abstract void DefineConstant(string name, RuntimeObject value);

        public abstract void DefineVariable(string name, RuntimeObject value);

        public virtual RuntimeObject GetConstant(string name)
        {
            return RuntimeObject.Undefined;
        }

        public abstract RuntimeObject CreateInstance();

        public abstract bool HasConstant(string name);

        public abstract IEnumerable<KeyValuePair<string, RuntimeObject>> GetConstants();

        internal abstract IDictionary<object, RuntimeObject> Init(RuntimeObject instance, [System.Runtime.InteropServices.Optional]KeyValuePair<object, RuntimeObject> initial);

        public static Prototype Merge(Prototype prototype1, Prototype prototype2)
        {
            var constants = System.Linq.Enumerable.Concat(prototype1.GetConstants(), prototype2.GetConstants());
            var variables = System.Linq.Enumerable.Concat(prototype1.GetVariables(), prototype2.GetVariables());
            var methods = System.Linq.Enumerable.Concat(prototype1.GetMethods(), prototype2.GetMethods());
            var prototype = new ObjectPrototype(constants, variables, methods);
            return prototype;
        }
#else
        public virtual void DefineConstant(string name, object value)
        {
            throw new System.Exception("Can't define constant inside " + GetType());
        }
#endif

    }
}
