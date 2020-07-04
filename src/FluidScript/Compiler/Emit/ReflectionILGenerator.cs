using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Reflection IL Generator
    /// </summary>
    public class ReflectionILGenerator : ILGenerator
    {
        /// <summary>
        /// Underlying ILGenerator
        /// </summary>
        protected readonly System.Reflection.Emit.ILGenerator Generator;
        /// <summary>
        /// Indicated whether to emit debug info
        /// </summary>
        public readonly bool EmitDebugInfo;

        /// <summary>
        /// New Reflection ILGenerator
        /// </summary>
        public ReflectionILGenerator(System.Reflection.Emit.ILGenerator generator, bool emitDebugInfo)
        {
            Generator = generator;
            EmitDebugInfo = emitDebugInfo;
        }

        ///<inheritdoc/>
        public override void Add()
        {
            Generator.Emit(OpCodes.Add);
        }

        ///<inheritdoc/>
        public override void BegineCatchBlock(Type exceptionType)
        {
            Generator.BeginCatchBlock(exceptionType);
        }

        ///<inheritdoc/>
        public override void BegineExceptionBlock()
        {
            Generator.BeginExceptionBlock();
        }

        ///<inheritdoc/>
        public override void BeginFaultBlock()
        {
            Generator.BeginFaultBlock();
        }

        ///<inheritdoc/>
        public override void BeginFilterBlock()
        {
            Generator.BeginExceptFilterBlock();
        }

        ///<inheritdoc/>
        public override void BeginFinallyBlock()
        {
            Generator.BeginFinallyBlock();
        }

        ///<inheritdoc/>
        public override void BitwiseAnd()
        {
            Generator.Emit(OpCodes.And);
        }

        ///<inheritdoc/>
        public override void BitwiseNot()
        {
            Generator.Emit(OpCodes.Not);
        }

        ///<inheritdoc/>
        public override void BitwiseOr()
        {
            Generator.Emit(OpCodes.Or);
        }

        ///<inheritdoc/>
        public override void BitwiseXor()
        {
            Generator.Emit(OpCodes.Xor);
        }

        ///<inheritdoc/>
        public override void Box(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Box, type);
        }

        ///<inheritdoc/>
        public override void Branch(ILLabel label)
        {
            Generator.Emit(OpCodes.Br, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfEqual(ILLabel label)
        {
            Generator.Emit(OpCodes.Beq, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfGreaterThan(ILLabel label)
        {
            Generator.Emit(OpCodes.Bgt, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfGreaterThanOrEqual(ILLabel label)
        {
            Generator.Emit(OpCodes.Bge, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfGreaterThanOrEqualUnsigned(ILLabel label)
        {
            Generator.Emit(OpCodes.Bge_Un, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfGreaterThanUnsigned(ILLabel label)
        {
            Generator.Emit(OpCodes.Bgt_Un, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfLessThan(ILLabel label)
        {
            Generator.Emit(OpCodes.Blt, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfLessThanOrEqual(ILLabel label)
        {
            Generator.Emit(OpCodes.Ble, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfLessThanOrEqualUnsigned(ILLabel label)
        {
            Generator.Emit(OpCodes.Ble_Un, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfLessThanUnsigned(ILLabel label)
        {
            Generator.Emit(OpCodes.Blt_Un, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfNotEqual(ILLabel label)
        {
            Generator.Emit(OpCodes.Bne_Un, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfNotZero(ILLabel label)
        {
            Generator.Emit(OpCodes.Brtrue, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void BranchIfZero(ILLabel label)
        {
            Generator.Emit(OpCodes.Brfalse, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void Breakpoint()
        {
            Generator.Emit(OpCodes.Break);
        }

        ///<inheritdoc/>
        public override void CallStatic(MethodBase method)
        {
            if (method is IMethodBase)
                method = ((IMethodBase)method).MethodBase;
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Call, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Call, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of MethodBase");
        }

        ///<inheritdoc/>
        public override void CallVirtual(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (method.IsStatic)
                throw new ArgumentNullException(nameof(method));
            if (method is IMethodBase)
                method = ((IMethodBase)method).MethodBase;
            if (method is ConstructorInfo)
            {
                if (method.IsVirtual)
                    Generator.Emit(OpCodes.Callvirt, (ConstructorInfo)method);
                else
                    Generator.Emit(OpCodes.Call, (ConstructorInfo)method);
            }
            else if (method is MethodInfo)
            {
                if (method.IsVirtual)
                    Generator.Emit(OpCodes.Callvirt, (MethodInfo)method);
                else
                    Generator.Emit(OpCodes.Call, (MethodInfo)method);
            }
            else
                throw new InvalidOperationException("unsupported subtype of MethodBase");
        }

        ///<inheritdoc/>
        public override void CastClass(Type type)
        {
            Generator.Emit(OpCodes.Castclass, type);
        }

        ///<inheritdoc/>
        public override void CompareEqual()
        {
            Generator.Emit(OpCodes.Ceq);
        }

        ///<inheritdoc/>
        public override void CompareGreaterThan()
        {
            Generator.Emit(OpCodes.Cgt);
        }

        ///<inheritdoc/>
        public override void CompareGreaterThanUnsigned()
        {
            Generator.Emit(OpCodes.Cgt_Un);
        }

        ///<inheritdoc/>
        public override void CompareLessThan()
        {
            Generator.Emit(OpCodes.Clt);
        }

        ///<inheritdoc/>
        public override void CompareLessThanUnsigned()
        {
            Generator.Emit(OpCodes.Clt_Un);
        }

        ///<inheritdoc/>
        public override void Complete()
        {
            Return();
        }

        ///<inheritdoc/>
        public override void ConvertToBool()
        {
            Generator.Emit(OpCodes.Conv_I1);
        }

        ///<inheritdoc/>
        public override void ConvertToByte()
        {
            Generator.Emit(OpCodes.Conv_I1);
        }

        ///<inheritdoc/>
        public override void ConvertToChar()
        {
            Generator.Emit(OpCodes.Conv_U2);
        }

        ///<inheritdoc/>
        public override void ConvertToDouble()
        {
            Generator.Emit(OpCodes.Conv_R8);
        }

        ///<inheritdoc/>
        public override void ConvertToInt16()
        {
            Generator.Emit(OpCodes.Conv_I2);
        }

        ///<inheritdoc/>
        public override void ConvertToInt32()
        {
            Generator.Emit(OpCodes.Conv_I4);
        }

        ///<inheritdoc/>
        public override void ConvertToInt64()
        {
            Generator.Emit(OpCodes.Conv_I8);
        }

        ///<inheritdoc/>
        public override void ConvertToSingle()
        {
            Generator.Emit(OpCodes.Conv_R4);
        }

        ///<inheritdoc/>
        public override void ConvertToUnsignedByte()
        {
            Generator.Emit(OpCodes.Conv_U1);
        }

        ///<inheritdoc/>
        public override void ConvertToUnsignedInt16()
        {
            Generator.Emit(OpCodes.Conv_U2);
        }

        ///<inheritdoc/>
        public override void ConvertToUnsignedInt32()
        {
            Generator.Emit(OpCodes.Conv_U4);
        }

        ///<inheritdoc/>
        public override void ConvertToUnsignedInt64()
        {
            Generator.Emit(OpCodes.Conv_U8);
        }

        ///<inheritdoc/>
        public override ILLabel CreateLabel()
        {
            return new ILLabel(Generator.DefineLabel());
        }

        ///<inheritdoc/>
        public override ILLocalVariable DeclareVariable(Type type, string name = null, bool pinned = false)
        {
            var localBuilder = Generator.DeclareLocal(type, pinned);
#if NETFRAMEWORK
            if (EmitDebugInfo && name != null)
            {
                localBuilder.SetLocalSymInfo(name);
            }
#endif
            return new ILLocalVariable(localBuilder, name);
        }

        ///<inheritdoc/>
        public override void DefineLabelPosition(ILLabel label)
        {
            Generator.MarkLabel(label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void Divide()
        {
            Generator.Emit(OpCodes.Div);
        }

        ///<inheritdoc/>
        public override void Duplicate()
        {
            Generator.Emit(OpCodes.Dup);
        }

        ///<inheritdoc/>
        public override void EndExceptionBlock()
        {
            Generator.EndExceptionBlock();
        }

        ///<inheritdoc/>
        public override void EndFilter()
        {
            Generator.Emit(OpCodes.Endfilter);
        }

        ///<inheritdoc/>
        public override void EndFinally()
        {
            Generator.Emit(OpCodes.Endfinally);
        }

        ///<inheritdoc/>
        public override void InitObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Initobj, type);
        }

        ///<inheritdoc/>
        public override void IsInstance(Type type)
        {
            Generator.Emit(OpCodes.Isinst, type);
        }

        ///<inheritdoc/>
        public override void Leave(ILLabel label)
        {
            Generator.Emit(OpCodes.Leave, label.UnderlyingLabel);
        }

        ///<inheritdoc/>
        public override void LoadAddressOfVariable(ILLocalVariable variable)
        {
            Generator.Emit(OpCodes.Ldloca, variable.UnderlyingLocal);
        }

        ///<inheritdoc/>
        public override void LoadArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(argumentIndex));
            switch (argumentIndex)
            {
                case 0:
                    Generator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Generator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Generator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Generator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (argumentIndex < 256)
                        Generator.Emit(OpCodes.Ldarg_S, (byte)argumentIndex);
                    else
                        Generator.Emit(OpCodes.Ldarg, (short)argumentIndex);
                    break;
            }
        }

        ///<inheritdoc/>
        public override void LoadAddressOfArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(argumentIndex));
            if (argumentIndex < 256)
                Generator.Emit(OpCodes.Ldarga_S, (byte)argumentIndex);
            else
                Generator.Emit(OpCodes.Ldarga, (short)argumentIndex);
        }
        ///<inheritdoc/>
        public override void LoadArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    Generator.Emit(OpCodes.Ldelem_I1);
                    break;
                case TypeCode.SByte:
                    Generator.Emit(OpCodes.Ldelem_U1);
                    break;
                case TypeCode.Int16:
                    Generator.Emit(OpCodes.Ldelem_I2);
                    break;
                case TypeCode.Char:
                case TypeCode.UInt16:
                    Generator.Emit(OpCodes.Ldelem_U2);
                    break;
                case TypeCode.Int32:
                    Generator.Emit(OpCodes.Ldelem_I4);
                    break;
                case TypeCode.UInt32:
                    Generator.Emit(OpCodes.Ldelem_U4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    Generator.Emit(OpCodes.Ldelem_I8);
                    break;
                case TypeCode.Single:
                    Generator.Emit(OpCodes.Ldelem_R4);
                    break;
                case TypeCode.Double:
                    Generator.Emit(OpCodes.Ldelem_R8);
                    break;
                default:
                    if (type.IsClass)
                        Generator.Emit(OpCodes.Ldelem_Ref);
                    else
                        Generator.Emit(OpCodes.Ldelem, type);
                    break;

            }
        }

        ///<inheritdoc/>
        public override void LoadArrayLength()
        {
            Generator.Emit(OpCodes.Ldlen);
        }

        ///<inheritdoc/>
        public override void LoadBool(bool value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value ? 1 : 0);
        }

        ///<inheritdoc/>
        public override void LoadByte(byte value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        ///<inheritdoc/>
        public override void LoadByte(sbyte value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        ///<inheritdoc/>
        public override void LoadChar(char value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        ///<inheritdoc/>
        public override void LoadDouble(double value)
        {
            Generator.Emit(OpCodes.Ldc_R8, value);
        }

        ///<inheritdoc/>
        public override void LoadField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.IsStatic)
                Generator.Emit(OpCodes.Ldsfld, field);
            else
                Generator.Emit(OpCodes.Ldfld, field);
        }

        ///<inheritdoc/>
        public override void LoadInt16(short value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        ///<inheritdoc/>
        public override void LoadInt16(ushort value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        ///<inheritdoc/>
        public override void LoadInt32(int value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        //private void LoadBit(sbyte value)
        //{
        //    switch (value)
        //    {
        //        case -1:
        //            Generator.Emit(OpCodes.Ldc_I4_M1);
        //            break;
        //        case 0:
        //            Generator.Emit(OpCodes.Ldc_I4_0);
        //            break;
        //        case 1:
        //            Generator.Emit(OpCodes.Ldc_I4_1);
        //            break;
        //        case 2:
        //            Generator.Emit(OpCodes.Ldc_I4_2);
        //            break;
        //        case 3:
        //            Generator.Emit(OpCodes.Ldc_I4_3);
        //            break;
        //        case 4:
        //            Generator.Emit(OpCodes.Ldc_I4_4);
        //            break;
        //        case 5:
        //            Generator.Emit(OpCodes.Ldc_I4_5);
        //            break;
        //        case 6:
        //            Generator.Emit(OpCodes.Ldc_I4_6);
        //            break;
        //        case 7:
        //            Generator.Emit(OpCodes.Ldc_I4_7);
        //            break;
        //        case 8:
        //            Generator.Emit(OpCodes.Ldc_I4_8);
        //            break;
        //    }
        //}

        ///<inheritdoc/>
        public override void LoadInt32(uint value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        ///<inheritdoc/>
        public override void LoadInt64(long value)
        {
            Generator.Emit(OpCodes.Ldc_I8, value);
        }

        ///<inheritdoc/>
        public override void LoadInt64(ulong value)
        {
            Generator.Emit(OpCodes.Ldc_I8, value);
        }

        ///<inheritdoc/>
        public override void LoadNull()
        {
            Generator.Emit(OpCodes.Ldnull);
        }

        ///<inheritdoc/>
        public override void LoadSingle(float value)
        {
            Generator.Emit(OpCodes.Ldc_R4, value);
        }

        ///<inheritdoc/>
        public override void LoadStaticMethodPointer(MethodBase method)
        {
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldftn, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldftn, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        ///<inheritdoc/>
        public override void LoadString(string value)
        {
            Generator.Emit(OpCodes.Ldstr, value);
        }

        ///<inheritdoc/>
        public override void LoadToken(Type type)
        {
            Generator.Emit(OpCodes.Ldtoken, type);
        }

        ///<inheritdoc/>
        public override void LoadToken(MethodBase method)
        {
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldtoken, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldtoken, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        ///<inheritdoc/>
        public override void LoadToken(FieldInfo field)
        {
            Generator.Emit(OpCodes.Ldtoken, field);
        }

        ///<inheritdoc/>
        public override void LoadVariable(ILLocalVariable variable)
        {
            Generator.Emit(OpCodes.Ldloc, variable.UnderlyingLocal);
        }

        ///<inheritdoc/>
        public override void LoadVirtualMethodPointer(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (method.IsStatic)
                throw new ArgumentException(nameof(method));
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldvirtftn, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldvirtftn, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        ///<inheritdoc/>
        public override void MarkSequencePoint(System.Diagnostics.SymbolStore.ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
#if NETFRAMEWORK
            Generator.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
#endif
        }

        /// <summary>
        /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="document"> The document for which the sequence point is being defined. </param>
        /// <param name="span"> The start and end positions which define the sequence point. </param>
        public void MarkSequencePoint(System.Diagnostics.SymbolStore.ISymbolDocumentWriter document, Compiler.Debugging.TextSpan span)
        {
            MarkSequencePoint(document, span.StartLine, span.StartColumn, span.EndLine, span.EndColumn);
        }

        ///<inheritdoc/>
        public override void Multiply()
        {
            Generator.Emit(OpCodes.Mul);
        }

        ///<inheritdoc/>
        public override void Negate()
        {
            Generator.Emit(OpCodes.Neg);
        }

        ///<inheritdoc/>
        public override void NewArray(Type type)
        {
            Generator.Emit(OpCodes.Newarr, type);
        }

        ///<inheritdoc/>
        public override void NewObject(ConstructorInfo constructor)
        {
            if (constructor is Generators.ConstructorGenerator)
            {
                Generator.Emit(OpCodes.Newobj, (ConstructorInfo)((Generators.ConstructorGenerator)constructor).MethodBase);
                return;
            }

            Generator.Emit(OpCodes.Newobj, constructor);
        }


        public void LoadFunction(MethodInfo method, Type delgateType)
        {
            Generator.Emit(OpCodes.Ldftn, method);
            Generator.Emit(OpCodes.Newobj, delgateType.GetConstructors()[0]);
        }

        ///<inheritdoc/>
        public override void NoOperation()
        {
            Generator.Emit(OpCodes.Nop);
        }

        ///<inheritdoc/>
        public override void Pop()
        {
            Generator.Emit(OpCodes.Pop);
        }

        ///<inheritdoc/>
        public override void Remainder()
        {
            Generator.Emit(OpCodes.Rem);
        }

        ///<inheritdoc/>
        public override void ReThrow()
        {
            Generator.Emit(OpCodes.Rethrow);
        }

        ///<inheritdoc/>
        public override void Return()
        {
            Generator.Emit(OpCodes.Ret);
        }

        ///<inheritdoc/>
        public override void ShiftLeft()
        {
            Generator.Emit(OpCodes.Shl);
        }

        ///<inheritdoc/>
        public override void ShiftRight()
        {
            Generator.Emit(OpCodes.Shr);
        }

        ///<inheritdoc/>
        public override void ShiftRightUnsigned()
        {
            Generator.Emit(OpCodes.Shr_Un);
        }

        ///<inheritdoc/>
        public override void StoreArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentNullException(nameof(argumentIndex));
            if (argumentIndex < 256)
                Generator.Emit(OpCodes.Starg_S, (byte)argumentIndex);
            else
                Generator.Emit(OpCodes.Starg, (short)argumentIndex);
        }

        ///<inheritdoc/>
        public override void StoreArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                    Generator.Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    Generator.Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.Char:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    Generator.Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    Generator.Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    Generator.Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    Generator.Emit(OpCodes.Stelem_R8);
                    break;
                default:
                    if (type.IsClass)
                        Generator.Emit(OpCodes.Stelem_Ref);
                    else
                        Generator.Emit(OpCodes.Stelem, type);
                    break;

            }
        }

        ///<inheritdoc/>
        public override void StoreField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.IsStatic)
                Generator.Emit(OpCodes.Stsfld, field);
            else
                Generator.Emit(OpCodes.Stfld, field);
        }

        ///<inheritdoc/>
        public override void StoreVariable(ILLocalVariable variable)
        {
            Generator.Emit(OpCodes.Stloc, variable.UnderlyingLocal);
        }

        ///<inheritdoc/>
        public override void Subtract()
        {
            Generator.Emit(OpCodes.Sub);
        }

        ///<inheritdoc/>
        public override void Switch(ILLabel[] labels)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            var reflectionLabels = new Label[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                reflectionLabels[i] = labels[i].UnderlyingLabel;
            }
            Generator.Emit(OpCodes.Switch, reflectionLabels);
        }

        ///<inheritdoc/>
        public override void Throw()
        {
            Generator.Emit(OpCodes.Throw);
        }

        ///<inheritdoc/>
        public override void Unbox(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Unbox, type);
        }

        ///<inheritdoc/>
        public override void UnboxObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Unbox_Any, type);
        }

        public void UsingNamespace(string namespaceName)
        {
            Generator.UsingNamespace(namespaceName);
        }
    }
}
