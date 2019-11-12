using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Metadata
{
    /// <summary>
    /// Prototype of current method or class
    /// </summary>
    public abstract class Prototype : RuntimeObject
    {
        public readonly Prototype Parent;
        public readonly ScopeContext Context;

        public Prototype(Prototype parent, ScopeContext context)
        {
            Parent = parent;
            Context = context;
        }

        public virtual Reflection.DeclaredMethod DeclareMethod(SyntaxTree.FunctionDeclaration declaration, SyntaxTree.BlockStatement body)
        {
            throw new System.Exception("Can't declare method inside " + GetType());
        }

        public virtual Reflection.DeclaredVariable DeclareLocalVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare local variable inside " + GetType());
        }

        public virtual void DefineConstant(string name, RuntimeObject value)
        {
            throw new System.Exception("Can't define constant inside " + GetType());
        }

        public virtual Reflection.DeclaredMethod GetMethod(string name, PrimitiveType[] primitiveType)
        {
            throw new NotImplementedException();
        }

        public virtual Reflection.DeclaredVariable DeclareVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare variable inside " + GetType());
        }

        internal virtual RuntimeObject GetConstant(string name)
        {
            throw new System.NotImplementedException();
        }

        public abstract IDictionary<string, Reflection.DeclaredVariable> Variables { get; }

        public Reflection.DeclaredVariable GetVariable(string name)
        {
            if (Variables != null && Variables.ContainsKey(name))
                return Variables[name];
            if (!ReferenceEquals(Parent,null))
            {
                return Parent.GetVariable(name);
            }
            return null;
        }

        internal abstract bool HasVariable(string name);

        public virtual void DefineVariable(string name, RuntimeObject value)
        {
            throw new System.Exception("Can't declare variable inside " + GetType());
        }

        public override RuntimeObject this[string name]
        {
            get
            {
                throw new System.Exception("Instance not found");
            }
        }

        public override RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            var types = args.Select(arg => arg.RuntimeType).ToArray();
            var method = GetMethod(name, types);
            if (method != null)
            {
                if (method.Delegate != null)
                {
                    return method.Delegate(args);
                }
            }
            return base.Call(name, args);
        }

        public override bool IsArray()
        {
            return true;
        }

        public override bool IsBool()
        {
            return false;
        }

        public override bool IsChar()
        {
            return false;
        }

        public override bool IsNull()
        {
            return false;
        }

        public override bool IsNumber()
        {
            return false;
        }

        public override bool IsString()
        {
            return true;
        }

        public override bool ToBool()
        {
            return false;
        }

        public override char ToChar()
        {
            return char.MinValue;
        }

        public override double ToDouble()
        {
            return double.NaN;
        }

        public override float ToFloat()
        {
            return float.NaN;
        }

        public override int ToInt32()
        {
            return int.MinValue;
        }

        public override double ToNumber()
        {
            return double.NaN;
        }
    }
}
