using System.Collections.Generic;

namespace FluidScript.Compiler.Metadata
{
    /// <summary>
    /// Represents BluePrint of Object contains method, fields or constants
    /// </summary>
    public abstract class Prototype
    {
        public readonly Prototype Parent;
        public readonly ScopeContext Context;
        public readonly string Name;

#if Runtime
        public static Prototype Create(System.Type type)
        {
            if (type == null)
                return null;
            if (type == typeof(object) || type == typeof(RuntimeObject))
                return ObjectPrototype.Default;
            var parent = type.DeclaringType != null ? Create(type.DeclaringType) : null;
            Prototype baseProto = Create(type.BaseType);
            IEnumerable<Reflection.DeclaredMember> members = Reflection.TypeHelper.GetMembers(type);
            return new ObjectPrototype(members, parent, baseProto, type.Name);
        }
#endif

        public Prototype(Prototype parent, string name, ScopeContext context)
        {
            Parent = parent;
            Name = name;
            Context = context;
        }

        internal virtual Reflection.DeclaredMethod DeclareMethod(string name, SyntaxTree.ArgumentInfo[] arguments, Emit.TypeName returnType, SyntaxTree.BodyStatement body)
        {
            throw new System.Exception("Can't declare method inside " + GetType());
        }

        internal virtual Reflection.DeclaredLocalVariable DeclareLocalVariable(string name, Emit.TypeName type, SyntaxTree.Expression expression, Reflection.VariableAttributes attribute = Reflection.VariableAttributes.Default)
        {
            throw new System.Exception("Can't declare local field inside " + GetType());
        }

        internal virtual Reflection.DeclaredField DeclareField(string name, Emit.TypeName type, SyntaxTree.Expression expression)
        {
            throw new System.Exception("Can't declare field inside " + GetType());
        }

        public abstract IEnumerable<Reflection.DeclaredField> GetFields();

        public abstract IEnumerable<Reflection.DeclaredMember> GetMembers();

        public abstract IEnumerable<Reflection.DeclaredMethod> GetMethods();

        public abstract bool HasMember(string name);

#if Runtime

        internal virtual void DeclareVariable(string name, SyntaxTree.Expression expression)
        {
            if (IsSealed)
                throw new System.Exception(string.Concat("can't declared a variable ", name, " inside " + Name));
            DeclareField(name, Emit.TypeName.Any, expression);
        }

        public bool IsSealed { get; internal set; }

        public abstract void DefineVariable(string name, RuntimeObject value);

        public abstract RuntimeObject CreateInstance();

        internal abstract Reflection.Instances Init(RuntimeObject instance, [System.Runtime.InteropServices.Optional]KeyValuePair<object, RuntimeObject> initial);

        public static Prototype Merge(Prototype prototype1, Prototype prototype2)
        {
            if (prototype1.Context == prototype2.Context)
            {
                return prototype2.Merge(prototype2);
            }
            if (prototype1.Context == ScopeContext.Local && prototype2.Context == ScopeContext.Type)
            {
                return Merge(prototype2, (FunctionPrototype)prototype1);
            }
            if (prototype2.Context == ScopeContext.Local && prototype1.Context == ScopeContext.Type)
            {
                return Merge(prototype1, (FunctionPrototype)prototype2);
            }
            throw new System.Exception("Can't Merge");
            //return new ObjectPrototype(constants, variables, methods, null, ObjectPrototype.Default, string.Concat(prototype1.Name, "_", prototype2.Name));
        }

        internal abstract Prototype Merge(Prototype prototype2);

        internal static Prototype Merge(Prototype prototype1, FunctionPrototype prototype2)
        {
            var fields = System.Linq.Enumerable.Select(prototype2.GetVariables(), item => new Reflection.DeclaredField(item.Name, item.ReflectedType)
            {
                ValueAtTop = item.ValueAtTop,
                DefaultValue = item.DefaultValue
            });
            IEnumerable<Reflection.DeclaredMember> members = System.Linq.Enumerable.Concat(fields, System.Linq.Enumerable.Concat(prototype2.GetMembers(), prototype1.GetMembers()));
            return new ObjectPrototype(members, null, ObjectPrototype.Default, string.Concat(prototype1.Name, "_", prototype2.Name));

        }
#endif

        public static implicit operator Prototype(System.Type type)
        {
            return Create(type);
        }

    }
}
