﻿using System;
using System.Collections.Generic;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Represents a generator of CIL bytes.
    /// </summary>
    public abstract class ILGenerator
    {
        private List<ILLocalVariable> temporaryVariables;
        /// <summary>
        /// Emits a return statement and finalizes the generated code.  Do not emit any more
        /// instructions after calling this method.
        /// </summary>
        public abstract void Complete();

        /// <summary>
        /// Pops the value from the top of the stack.
        /// </summary>
        public abstract void Pop();

        /// <summary>
        /// Duplicates the value on the top of the stack.
        /// </summary>
        public abstract void Duplicate();

        /// <summary>
        /// Creates a label without setting its position.
        /// </summary>
        /// <returns> A new label. </returns>
        public abstract ILLabel CreateLabel();

        /// <summary>
        /// Defines the position of the given label.
        /// </summary>
        /// <param name="label"> The label to define. </param>
        public abstract void DefineLabelPosition(ILLabel label);

        /// <summary>
        /// Creates a label and sets its position.
        /// </summary>
        /// <returns> A new label. </returns>
        public ILLabel DefineLabelPosition()
        {
            var label = CreateLabel();
            DefineLabelPosition(label);
            return label;
        }

        /// <summary>
        /// Unconditionally branches to the given label.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void Branch(ILLabel label);

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero, false or
        /// null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfZero(ILLabel label);

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfNotZero(ILLabel label);

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero, false or
        /// null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public void BranchIfFalse(ILLabel label)
        {
            BranchIfZero(label);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public void BranchIfTrue(ILLabel label)
        {
            BranchIfNotZero(label);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is zero, false or
        /// null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public void BranchIfNull(ILLabel label)
        {
            BranchIfZero(label);
        }

        /// <summary>
        /// Branches to the given label if the value on the top of the stack is non-zero, true or
        /// non-null.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public void BranchIfNotNull(ILLabel label)
        {
            BranchIfNotZero(label);
        }

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfEqual(ILLabel label);

        /// <summary>
        /// Branches to the given label if the two values on the top of the stack are not equal.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfNotEqual(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfGreaterThan(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than the second
        /// value on the stack.  If the operands are integers then they are treated as if they are
        /// unsigned.  If the operands are floating point numbers then a NaN value will trigger a
        /// branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfGreaterThanUnsigned(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfGreaterThanOrEqual(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is greater than or equal to
        /// the second value on the stack.  If the operands are integers then they are treated as
        /// if they are unsigned.  If the operands are floating point numbers then a NaN value will
        /// trigger a branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfGreaterThanOrEqualUnsigned(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfLessThan(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than the second
        /// value on the stack.  If the operands are integers then they are treated as if they are
        /// unsigned.  If the operands are floating point numbers then a NaN value will trigger a
        /// branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfLessThanUnsigned(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfLessThanOrEqual(ILLabel label);

        /// <summary>
        /// Branches to the given label if the first value on the stack is less than or equal to
        /// the second value on the stack.  If the operands are integers then they are treated as
        /// if they are unsigned.  If the operands are floating point numbers then a NaN value will
        /// trigger a branch.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void BranchIfLessThanOrEqualUnsigned(ILLabel label);

        /// <summary>
        /// Returns from the current method.  A value is popped from the stack and used as the
        /// return value.
        /// </summary>
        public abstract void Return();

        /// <summary>
        /// Creates a jump table.  A value is popped from the stack - this value indicates the
        /// index of the label in the <paramref name="labels"/> array to jump to.
        /// </summary>
        /// <param name="labels"> A array of labels. </param>
        public abstract void Switch(ILLabel[] labels);

        /// <summary>
        /// Declares a new local variable.
        /// </summary>
        /// <param name="type"> The type of the local variable. </param>
        /// <param name="name"> The name of the local variable. Can be <c>null</c>. </param>
        /// <param name="pinned">true to pin the object in memory; otherwise, false. </param>
        /// <returns> A new local variable. </returns>
        public abstract ILLocalVariable DeclareVariable(Type type, string name = null, bool pinned = false);

        /// <summary>
        /// Pushes the value of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose value will be pushed. </param>
        public abstract void LoadVariable(ILLocalVariable variable);

        /// <summary>
        /// Pushes the address of the given variable onto the stack.
        /// </summary>
        /// <param name="variable"> The variable whose address will be pushed. </param>
        public abstract void LoadAddressOfVariable(ILLocalVariable variable);

        /// <summary>
        /// Pops the value from the top of the stack and stores it in the given local variable.
        /// </summary>
        /// <param name="variable"> The variable to store the value. </param>
        public abstract void StoreVariable(ILLocalVariable variable);

        /// <summary>
        /// Pushes the value of the method argument with the given index onto the stack.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to push onto the stack. </param>
        public abstract void LoadArgument(int argumentIndex);

        /// <summary>
        /// Pushes the address of the given argument onto the stack.
        /// </summary>
        /// <param name="argumentIndex"> The parameter index whose address will be pushed. </param>
        public abstract void LoadAddressOfArgument(int argumentIndex);

        /// <summary>
        /// Pops a value from the stack and stores it in the method argument with the given index.
        /// </summary>
        /// <param name="argumentIndex"> The index of the argument to store into. </param>
        public abstract void StoreArgument(int argumentIndex);

        /// <summary>
        /// Creates temp ILVariab le
        /// </summary>
        public ILLocalVariable CreateTemporaryVariable(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (temporaryVariables != null)
            {
                for (int i = 0; i < temporaryVariables.Count; i++)
                {
                    ILLocalVariable temporary = temporaryVariables[i];
                    if (temporary.Type == type)
                    {
                        temporaryVariables.RemoveAt(i);
                        return temporary;
                    }
                }
            }
            // Create a new temporary variable
            return DeclareVariable(type);
        }

        /// <summary>
        /// Indicates that the given temporary variable is no longer needed.
        /// </summary>
        /// <param name="variable"> The temporary variable created using CreateTemporaryVariable(). </param>
        public void ReleaseTemporaryVariable(ILLocalVariable variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));
            if (temporaryVariables == null)
                temporaryVariables = new List<ILLocalVariable>();
            temporaryVariables.Add(variable);
        }

        /// <summary>
        /// Pushes <c>null</c> onto the stack.
        /// </summary>
        public abstract void LoadNull();

        /// <summary>
        /// Pushes a bool constant value onto the stack.
        /// </summary>
        /// <param name="value"> The bool to push onto the stack. </param>
        public abstract void LoadBool(bool value);

        /// <summary>
        /// Pushes a unsigned byte constant value onto the stack.
        /// </summary>
        /// <param name="value"> The unsigned byte to push onto the stack. </param>
        public abstract void LoadByte(byte value);

        /// <summary>
        /// Pushes a byte constant value onto the stack.
        /// </summary>
        /// <param name="value"> The signed byte to push onto the stack. </param>
        public abstract void LoadByte(sbyte value);

        /// <summary>
        /// Pushes a short constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 16 bit integer to push onto the stack. </param>
        public abstract void LoadInt16(short value);

        /// <summary>
        /// Pushes a unsigned short constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 16 bit unsigned integer to push onto the stack. </param>
        public abstract void LoadInt16(ushort value);

        /// <summary>
        /// Pushes a int constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 32 bit integer to push onto the stack. </param>
        public abstract void LoadInt32(int value);

        /// <summary>
        /// Pushes a unsigned int constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 32 bit unsigned integer to push onto the stack. </param>
        public abstract void LoadInt32(uint value);

        /// <summary>
        /// Pushes a long constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 64 bit integer to push onto the stack. </param>
        public abstract void LoadInt64(long value);

        /// <summary>
        /// Pushes a unsigned long constant value onto the stack.
        /// </summary>
        /// <param name="value"> The 64 bit unsigned integer to push onto the stack. </param>
        public abstract void LoadInt64(ulong value);

        /// <summary>
        /// Pushes a float constant value onto the stack.
        /// </summary>
        /// <param name="value"> The float to push onto the stack. </param>
        public abstract void LoadSingle(float value);

        /// <summary>
        /// Pushes a double constant value onto the stack.
        /// </summary>
        /// <param name="value"> The double to push onto the stack. </param>
        public abstract void LoadDouble(double value);

        /// <summary>
        /// Pushes a char constant value onto the stack.
        /// </summary>
        /// <param name="value"> The char to push onto the stack. </param>
        public abstract void LoadChar(char value);

        /// <summary>
        /// Pushes a string constant value onto the stack.
        /// </summary>
        /// <param name="value"> The string to push onto the stack. </param>
        public abstract void LoadString(string value);

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is equal to the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public abstract void CompareEqual();

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public abstract void CompareGreaterThan();

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is greater than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public abstract void CompareGreaterThanUnsigned();

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>0</c> if one or both
        /// of the arguments are <c>NaN</c>.
        /// </summary>
        public abstract void CompareLessThan();

        /// <summary>
        /// Pops two values from the stack, compares, then pushes <c>1</c> if the first argument
        /// is less than the second, or <c>0</c> otherwise.  Produces <c>1</c> if one or both
        /// of the arguments are <c>NaN</c>.  Integers are considered to be unsigned.
        /// </summary>
        public abstract void CompareLessThanUnsigned();

        #region Implic Calls
        #endregion

        /// <summary>
        /// Pops two values from the stack, adds them together, then pushes the result to the
        /// stack.
        /// </summary>
        public abstract void Add();

        /// <summary>
        /// Pops two values from the stack, subtracts the second from the first, then pushes the
        /// result to the stack.
        /// </summary>
        public abstract void Subtract();

        /// <summary>
        /// Pops two values from the stack, multiplies them together, then pushes the
        /// result to the stack.
        /// </summary>
        public abstract void Multiply();

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// result to the stack.
        /// </summary>
        public abstract void Divide();

        /// <summary>
        /// Pops two values from the stack, divides the first by the second, then pushes the
        /// remainder to the stack.
        /// </summary>
        public abstract void Remainder();

        /// <summary>
        /// Pops a value from the stack, negates it, then pushes it back onto the stack.
        /// </summary>
        public abstract void Negate();

        /// <summary>
        /// Pops two values from the stack, ANDs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public abstract void BitwiseAnd();

        /// <summary>
        /// Pops two values from the stack, ORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public abstract void BitwiseOr();

        /// <summary>
        /// Pops two values from the stack, XORs them together, then pushes the result to the
        /// stack.
        /// </summary>
        public abstract void BitwiseXor();

        /// <summary>
        /// Pops a value from the stack, inverts it, then pushes the result to the stack.
        /// </summary>
        public abstract void BitwiseNot();

        /// <summary>
        /// Pops two values from the stack, shifts the first to the left, then pushes the result
        /// to the stack.
        /// </summary>
        public abstract void ShiftLeft();

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is preserved.
        /// </summary>
        public abstract void ShiftRight();

        /// <summary>
        /// Pops two values from the stack, shifts the first to the right, then pushes the result
        /// to the stack.  The sign bit is not preserved.
        /// </summary>
        public abstract void ShiftRightUnsigned();

        /// <summary>
        /// Pops a value from the stack, converts it to an object reference, then pushes it back onto
        /// the stack.
        /// </summary>
        /// <param name="type"> The type of value to box.  This should be a value type. </param>
        public abstract void Box(Type type);


        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the
        /// address, then pushes that address onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public abstract void Unbox(Type type);

        /// <summary>
        /// Pops an object reference (representing a boxed value) from the stack, extracts the value,
        /// then pushes the value onto the stack.
        /// </summary>
        /// <param name="type"> The type of the boxed value.  This should be a value type. </param>
        public abstract void UnboxObject(Type type);

        /// <summary>
        /// Pops a value from the stack, converts it to a bool, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToBool();

        /// <summary>
        /// Pops a value from the stack, converts it to a signed byte, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToByte();

        /// <summary>
        /// Pops a value from the stack, converts it to a unsigned byte, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToUnsignedByte();

        /// <summary>
        /// Pops a value from the stack, converts it to a signed short, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToInt16();

        /// <summary>
        /// Pops a value from the stack, converts it to a unsigned short, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToUnsignedInt16();

        /// <summary>
        /// Pops a value from the stack, converts it to a signed integer, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToInt32();

        /// <summary>
        /// Pops a value from the stack, converts it to a unsigned integer, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToUnsignedInt32();

        /// <summary>
        /// Pops a value from the stack, converts it to a signed long, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToInt64();

        /// <summary>
        /// Pops a value from the stack, converts it to a unsigned long, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToUnsignedInt64();

        /// <summary>
        /// Pops a value from the stack, converts it to a char, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToChar();

        /// <summary>
        /// Pops a value from the stack, converts it to a float, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToSingle();

        /// <summary>
        /// Pops a value from the stack, converts it to a double, then pushes it back onto
        /// the stack.
        /// </summary>
        public abstract void ConvertToDouble();

        /// <summary>
        /// Pops the constructor arguments off the stack and creates a new instance of the object.
        /// </summary>
        /// <param name="constructor"> The constructor that is used to initialize the object. </param>
        public abstract void NewObject(System.Reflection.ConstructorInfo constructor);

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).  Identical to CallStatic() if the method is a static
        /// method (or is declared on a value type) or CallVirtual() otherwise.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        public void Call(System.Reflection.MethodBase method)
        {
            if (method.IsStatic == true || method.DeclaringType.IsValueType == true)
                CallStatic(method);
            else
                CallVirtual(method);
        }

        /// <summary>
        /// Call delegate method
        /// </summary>
        /// <param name="del"> The delegate to call. </param>
        public void Call(Delegate del)
        {
            Call(del.Method);
        }

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).  This operation can be used to call instance methods,
        /// but virtual overrides will not be called and a null check will not be performed at the
        /// callsite.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        public abstract void CallStatic(System.Reflection.MethodBase method);

        /// <summary>
        /// Pops the method arguments off the stack, calls the given method, then pushes the result
        /// to the stack (if there was one).  This operation cannot be used to call static methods.
        /// Virtual overrides are obeyed and a null check is performed.
        /// </summary>
        /// <param name="method"> The method to call. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public abstract void CallVirtual(System.Reflection.MethodBase method);

        /// <summary>
        /// Pushes the value of the given field onto the stack.
        /// </summary>
        /// <param name="field"> The field whose value will be pushed. </param>
        public abstract void LoadField(System.Reflection.FieldInfo field);

        /// <summary>
        /// Pushes the value of the given field address onto the stack.
        /// </summary>
        /// <param name="field"> The field whose value will be pushed. </param>
        public abstract void LoadFieldAddress(System.Reflection.FieldInfo field);

        /// <summary>
        /// Pops a value off the stack and stores it in the given field.
        /// </summary>
        /// <param name="field"> The field to modify. </param>
        public abstract void StoreField(System.Reflection.FieldInfo field);

        /// <summary>
        /// Pops an object off the stack, checks that the object inherits from or implements the
        /// given type, and pushes the object onto the stack if the check was successful or
        /// throws an InvalidCastException if the check failed.
        /// </summary>
        /// <param name="type"> The type of the class the object inherits from or the interface the
        /// object implements. </param>
        public abstract void CastClass(Type type);

        /// <summary>
        /// Pops an object off the stack, checks that the object inherits from or implements the
        /// given type, and pushes either the object (if the check was successful) or <c>null</c>
        /// (if the check failed) onto the stack.
        /// </summary>
        /// <param name="type"> The type of the class the object inherits from or the interface the
        /// object implements. </param>
        public abstract void IsInstance(Type type);

        /// <summary>
        /// Pushes a RuntimeTypeHandle corresponding to the given type onto the evaluation stack.
        /// </summary>
        /// <param name="type"> The type to convert to a RuntimeTypeHandle. </param>
        public abstract void LoadToken(Type type);

        /// <summary>
        /// Pushes a RuntimeMethodHandle corresponding to the given method onto the evaluation
        /// stack.
        /// </summary>
        /// <param name="method"> The method to convert to a RuntimeMethodHandle. </param>
        public abstract void LoadToken(System.Reflection.MethodBase method);

        /// <summary>
        /// Pushes a RuntimeFieldHandle corresponding to the given field onto the evaluation stack.
        /// </summary>
        /// <param name="field"> The type to convert to a RuntimeFieldHandle. </param>
        public abstract void LoadToken(System.Reflection.FieldInfo field);

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  The virtual qualifier will be ignored, if present.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        public abstract void LoadStaticMethodPointer(System.Reflection.MethodBase method);

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the
        /// evaluation stack.  This method cannot be used to retrieve a pointer to a static method.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        /// <exception cref="ArgumentException"> The method is static. </exception>
        public abstract void LoadVirtualMethodPointer(System.Reflection.MethodBase method);

        /// <summary>
        /// Pushes a pointer to the native code implementing the given method onto the evaluation
        /// stack.  This method is identical to LoadStaticMethodPointer() if the method is a static
        /// method (or is declared on a value type) or LoadVirtualMethodPointer() otherwise.
        /// </summary>
        /// <param name="method"> The method to retrieve a pointer for. </param>
        public void LoadMethodPointer(System.Reflection.MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (method.IsStatic == true || method.DeclaringType.IsValueType == true)
                LoadStaticMethodPointer(method);
            else
                LoadVirtualMethodPointer(method);
        }

        /// <summary>
        /// Pops a managed or native pointer off the stack and initializes the referenced type with
        /// zeros.
        /// </summary>
        /// <param name="type"> The type the pointer on the top of the stack is pointing to. </param>
        public abstract void InitObject(Type type);

        /// <summary>
        /// Pops the size of the array off the stack and pushes a new array of the given type onto
        /// the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public abstract void NewArray(Type type);

        /// <summary>
        /// Pops the array and index off the stack and pushes the element value onto the stack.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public abstract void LoadArrayElement(Type type);

        /// <summary>
        /// Pops the array, index and value off the stack and stores the value in the array.
        /// </summary>
        /// <param name="type"> The element type. </param>
        public abstract void StoreArrayElement(Type type);

        /// <summary>
        /// Pops an array off the stack and pushes the length of the array onto the stack.
        /// </summary>
        public abstract void LoadArrayLength();

        /// <summary>
        /// Pops an exception object off the stack and throws the exception.
        /// </summary>
        public abstract void Throw();


        /// <summary>
        /// Re-throws the current exception.
        /// </summary>
        public abstract void ReThrow();

        /// <summary>
        /// Begins a try-catch-finally block.  After issuing this instruction any following
        /// instructions are conceptually within the try block.
        /// </summary>
        public abstract void BegineExceptionBlock();

        /// <summary>
        /// Ends a try-catch-finally block.
        /// </summary>
        public abstract void EndExceptionBlock();

        /// <summary>
        /// Begins a catch block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        /// <param name="exceptionType"> The type of exception to handle. </param>
        public abstract void BegineCatchBlock(Type exceptionType);

        /// <summary>
        /// Begins a finally block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public abstract void BeginFinallyBlock();

        /// <summary>
        /// Begins a filter block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public abstract void BeginFilterBlock();

        /// <summary>
        /// Begins a fault block.  BeginExceptionBlock() must have already been called.
        /// </summary>
        public abstract void BeginFaultBlock();

        /// <summary>
        /// Unconditionally branches to the given label.  Unlike the regular branch instruction,
        /// this instruction can exit out of try, filter and catch blocks.
        /// </summary>
        /// <param name="label"> The label to branch to. </param>
        public abstract void Leave(ILLabel label);

        /// <summary>
        /// This instruction can be used from within a finally block to resume the exception
        /// handling process.  It is the only valid way of leaving a finally block.
        /// </summary>
        public abstract void EndFinally();

        /// <summary>
        /// This instruction can be used from within a fault block to resume the exception
        /// handling process.  It is the only valid way of leaving a fault block.
        /// </summary>
        public void EndFault()
        {
            EndFinally();
        }

        /// <summary>
        /// This instruction can be used from within a filter block to indicate whether the
        /// exception will be handled.  It pops an integer from the stack which should be <c>0</c>
        /// to continue searching for an exception handler or <c>1</c> to use the handler
        /// associated with the filter.  EndFilter() must be called at the end of a filter block.
        /// </summary>
        public abstract void EndFilter();

        /// <summary>
        /// Triggers a breakpoint in an attached debugger.
        /// </summary>
        public abstract void Breakpoint();

        /// <summary>
        /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="document"> The document for which the sequence point is being defined. </param>
        /// <param name="startLine"> The line where the sequence point begins. </param>
        /// <param name="startColumn"> The column in the line where the sequence point begins. </param>
        /// <param name="endLine"> The line where the sequence point ends. </param>
        /// <param name="endColumn"> The column in the line where the sequence point ends. </param>
        public abstract void MarkSequencePoint(System.Diagnostics.SymbolStore.ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn);

        /// <summary>
        /// Does nothing.
        /// </summary>
        public abstract void NoOperation();
    }
}
