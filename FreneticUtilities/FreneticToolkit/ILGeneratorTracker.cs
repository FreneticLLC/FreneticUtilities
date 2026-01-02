//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

#if DEBUG
#define VALIDATE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using FreneticUtilities.FreneticExtensions;

#pragma warning disable CA1822 // Mark members as static
namespace FreneticUtilities.FreneticToolkit;

/// <summary>Tracks generated IL.</summary>
public class ILGeneratorTracker
{
    /// <summary>Simple tiny helper, creates a delegate that constructs an object via the given constructor.</summary>
    public static Delegate CreateConstructorFunc(ConstructorInfo ctor, Type[] inTypes)
    {
        DynamicMethod dm = new(ctor.DeclaringType.Name + "_Ctor", ctor.DeclaringType, inTypes, true);
        ILGenerator il = dm.GetILGenerator();
        for (int i = 0; i < inTypes.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i);
        }
        il.Emit(OpCodes.Newobj, ctor);
        il.Emit(OpCodes.Ret);
        return dm.CreateDelegate(ctor.DeclaringType);
    }

    /// <summary>Creates the instance to wrap an MS base object.</summary>
    public ILGeneratorTracker(ILGenerator _internal, MethodInfo _method, string _name = null)
    {
        Internal = _internal;
        Parameters = [.. _method.GetParameters().Select(p => p.ParameterType)];
        if (!_method.IsStatic)
        {
            Parameters = new Type[1] { _method.DeclaringType }.JoinWith(Parameters);
        }
        Name = _name ?? _method.Name;
        PostInit();
    }

    /// <summary>Creates the instance to wrap an MS base object.</summary>
    public ILGeneratorTracker(ILGenerator _internal, ConstructorInfo _method, string _name = null)
    {
        Internal = _internal;
        Parameters = new Type[1] { _method.DeclaringType }.JoinWith([.. _method.GetParameters().Select(p => p.ParameterType)]);
        Name = _name ?? $"Constructor_{_method.DeclaringType.Name}";
        PostInit();
    }

    /// <summary>Creates the instance to wrap an MS base object.</summary>
    public ILGeneratorTracker(ILGenerator _internal, Type[] _params, string _name = "Unnamed")
    {
        Internal = _internal;
        Parameters = _params;
        Name = _name;
        PostInit();
    }

    [Conditional("VALIDATE")]
    private void PostInit()
    {
        Comment($"Name: {Name}, Params: {string.Join(", ", Parameters.Select(p => p.FullName))}");
    }

    /// <summary>Optional name for the generator.</summary>
    public string Name;

    /// <summary>Internal generator.</summary>
    public ILGenerator Internal;

    /// <summary>The parameters to the method being worked on.</summary>
    public Type[] Parameters;

    /// <summary>Action to show error output.</summary>
    public Action<string> Error = (str) =>
    {
        string[] bits = str.SplitFast('\0');
        Console.Error.Write(bits[0]);
        for (int i = 1; i < bits.Length; i++)
        {
            switch (bits[i][0])
            {
                case 'E':
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 'M':
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.Error.Write(bits[i][1..]);
        }
        Console.Error.WriteLine();
    };

    /// <summary>Shows an error and automatically formats around it.</summary>
    public void DoErrorDirect(string message)
    {
        Error($"{nameof(ILGeneratorTracker)} '{Name}' Error: {BaseCode}{message}\n{BaseCode}For code: {MinorCode}{Stringify()}\n{BaseCode}Stack: {MinorCode}{Environment.StackTrace}");
    }

    /// <summary>Optional text format codes.</summary>
    public string EmphasizeCode = "\0E", BaseCode = "\0B", MinorCode = "\0M";

#if VALIDATE
    /// <summary>All codes generated. Only has a value when compiled in DEBUG mode.</summary>
    public List<KeyValuePair<string, object>> Codes = [];

    /// <summary>Stack tracker, for validation.</summary>
    public Stack<Type> StackTypes = new();

    /// <summary>Local variables.</summary>
    public Dictionary<int, LocalBuilder> Locals = [];
#endif

    /// <summary>Gives a warning if stack size is not the exact correct size.</summary>
    /// <param name="situation">The current place in the code that requires a validation.</param>
    /// <param name="expected">The expected stack size.</param>
    [Conditional("VALIDATE")]
    public void ValidateStackSizeIs(string situation, int expected)
    {
#if VALIDATE
        if (StackTypes.Count != expected)
        {
            CommentStack();
            DoErrorDirect($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackTypes.Count}{BaseCode} but should be exactly {EmphasizeCode}{expected}");
        }
#endif
    }

    /// <summary>Gives a warning if stack size is not at least the given size.</summary>
    /// <param name="situation">The current place in the code that requires a validation.</param>
    /// <param name="expected">The expected minimum stack size.</param>
    [Conditional("VALIDATE")]
    public void ValidateStackSizeIsAtLeast(string situation, int expected)
    {
#if VALIDATE
        if (StackTypes.Count < expected)
        {
            CommentStack();
            DoErrorDirect($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackTypes.Count}{BaseCode} but should be at least {EmphasizeCode}{expected}");
        }
#endif
    }

    /// <summary>Gives a warning if stack size is not at most the given size.</summary>
    /// <param name="situation">The current place in the code that requires a validation.</param>
    /// <param name="expected">The expected maxium stack size.</param>
    [Conditional("VALIDATE")]
    public void ValidateStackSizeIsAtMost(string situation, int expected)
    {
#if VALIDATE
        if (StackTypes.Count > expected)
        {
            CommentStack();
            DoErrorDirect($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackTypes.Count}{BaseCode} but should be at most {EmphasizeCode}{expected}");
        }
#endif
    }

    /// <summary>Creates a string of all the generated CIL code.</summary>
    /// <returns>Generated CIL code string.</returns>
    public string Stringify()
    {
#if DEBUG
        StringBuilder fullResult = new();
        foreach (KeyValuePair<string, object> code in Codes)
        {
            if (code.Key == "minor")
            {
                fullResult.Append($"{BaseCode}(Minor){MinorCode}: {code.Value}\n");
            }
            else
            {
                fullResult.Append($"{EmphasizeCode}{code.Key}{MinorCode}: {code.Value}\n");
            }
        }
        return fullResult.ToString();
#else
            return "(Generator Not Tracked)";
#endif
    }

    /// <summary>
    /// When compiled in DEBUG mode, adds a code value to the "Codes" list.
    /// When compiled with VALIDATE set, validates the new opcode.
    /// </summary>
    /// <param name="code">The OpCode used (or 'nop' for special comments).</param>
    /// <param name="val">The value attached to the opcode, if any.</param>
    /// <param name="typeName">The special code type name, if any.</param>
    [Conditional("VALIDATE")]
    public void AddCode(OpCode code, object val, string typeName = null)
    {
#if DEBUG
        Codes.Add(new KeyValuePair<string, object>(typeName ?? code.ToString().ToLowerFast(), val));
#endif
        Validator(code, val);
    }

    /// <summary>Validation call for stack size wrangling.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="val">The object value.</param>
    /// <param name="altParams">The number of parameters if GetParameters() is not stable (can be invalid for non-generated methods).</param>
    [Conditional("VALIDATE")]
    public void Validator(OpCode code, object val, int? altParams = null)
    {
#if VALIDATE
        if (code == OpCodes.Nop)
        {
            // Do nothing
        }
        else if (code == OpCodes.Ret)
        {
            ValidateStackSizeIsAtMost("return op", 1);
            StackTypes.Clear();
        }
        else if (code == OpCodes.Call || code == OpCodes.Callvirt)
        {
            if (val is not MethodInfo method)
            {
                DoErrorDirect($"Invalid call (code {code}, to object {val}) - not a method reference");
            }
            else
            {
                if (method.IsAbstract && code == OpCodes.Call)
                {
                    DoErrorDirect($"Invalid call (code {code}, to object {val}) - method is abstract, cannot direct call, use callvirt");
                }
                int paramCount = altParams ?? method.GetParameters().Length;
                int fullParamCount = paramCount;
                if (!method.IsStatic)
                {
                    fullParamCount++;
                }
                ValidateStackSizeIsAtLeast($"call opcode {code}", fullParamCount);
                if (altParams is not null)
                {
                    StackSizeChange(-paramCount);
                }
                else
                {
                    for (int i = 0; i < paramCount && StackTypes.Any(); i++)
                    {
                        Type top = StackTypes.Pop();
                        Type paramType = method.GetParameters()[paramCount - (i + 1)].ParameterType;
                        if (!paramType.IsAssignableFrom(top) && !top.IsAssignableFrom(paramType))
                        {
                            DoErrorDirect($"Invalid call parameter {(paramCount - (i + 1))}: expected {paramType.FullName} but stack holds {top.FullName}");
                        }
                    }
                }
                if (!method.IsStatic && StackTypes.Any())
                {
                    Type top = StackTypes.Pop();
                    if (!method.DeclaringType.IsAssignableFrom(top) && !top.IsAssignableFrom(method.DeclaringType))
                    {
                        DoErrorDirect($"Invalid call self-object parameter: expected {method.DeclaringType.FullName} but stack holds {top.FullName}");
                    }
                }
                StackSizeChange(0);
                if (method.ReturnType != typeof(void))
                {
                    StackPush(method.ReturnType);
                }
            }
        }
        else if (code == OpCodes.Newobj)
        {
            if (val is not ConstructorInfo method)
            {
                DoErrorDirect($"Invalid NEWOBJ (code {code}, to object {val}) - not a constructor reference");
            }
            else
            {
                int paramCount = altParams ?? method.GetParameters().Length;
                ValidateStackSizeIsAtLeast("opcode newobj", paramCount);
                if (altParams is not null)
                {
                    StackSizeChange(-paramCount);
                }
                else
                {
                    for (int i = 0; i < paramCount && StackTypes.Any(); i++)
                    {
                        Type top = StackTypes.Pop();
                        Type paramType = method.GetParameters()[paramCount - (i + 1)].ParameterType;
                        if (!paramType.IsAssignableFrom(top) && !top.IsAssignableFrom(paramType))
                        {
                            DoErrorDirect($"Invalid call parameter {(paramCount - (i + 1))}: expected {paramType.FullName} but stack holds {top.FullName}");
                        }
                    }
                }
                StackPush(method.DeclaringType);
            }
        }
        else if (code == OpCodes.Ldfld)
        {
            if (val is not FieldInfo fld)
            {
                DoErrorDirect($"Invalid Ldfld (code {code}, to object {val}) - not a Field reference ({val}={val?.GetType()?.FullName})");
            }
            else if (fld.IsStatic)
            {
                DoErrorDirect($"Invalid Ldfld (code {code}, to object {val}) - field {fld.Name} is static");
            }
            else
            {
                ValidateStackSizeIsAtLeast($"opcode Ldfld", 1);
                Type top = StackTypes.Pop();
                StackSizeChange(0);
                if (!top.IsAssignableFrom(fld.DeclaringType) && !fld.DeclaringType.IsAssignableFrom(top))
                {
                    DoErrorDirect($"Invalid Ldfld (code {code}, to object {val}) parameter - field belongs to type {fld.DeclaringType.FullName} but stack holds {top.FullName}");
                }
                StackPush(fld.FieldType);
            }
        }
        else if (code == OpCodes.Stfld)
        {
            if (val is not FieldInfo fld)
            {
                DoErrorDirect($"Invalid Stfld (code {code}, to object {val}) - not a Field reference ({val}={val?.GetType()?.FullName})");
            }
            else if (fld.IsStatic)
            {
                DoErrorDirect($"Invalid Stfld (code {code}, to object {val}) - field {fld.Name} is static");
            }
            else
            {
                ValidateStackSizeIsAtLeast($"opcode Stfld", 2);
                Type toPush = StackTypes.Pop();
                Type toPushOnto = StackTypes.Pop();
                StackSizeChange(0);
                if (!toPush.IsAssignableFrom(fld.FieldType) && !fld.FieldType.IsAssignableFrom(toPush))
                {
                    DoErrorDirect($"Invalid Stfld (code {code}, to object {val}) parameter - field is type {fld.FieldType.FullName} but stack holds {toPush.FullName}");
                }
                if (!toPushOnto.IsAssignableFrom(fld.DeclaringType) && !fld.DeclaringType.IsAssignableFrom(toPushOnto))
                {
                    DoErrorDirect($"Invalid Stfld (code {code}, to object {val}) parameter - field belongs to type {fld.DeclaringType.FullName} but stack holds {toPushOnto.FullName}");
                }
            }
        }
        else if (code == OpCodes.Ldarg_0 || code == OpCodes.Ldarg_1 || code == OpCodes.Ldarg_2 || code == OpCodes.Ldarg_3 || code == OpCodes.Ldarg_S || code == OpCodes.Ldarg)
        {
            int argIndex;
            if (code == OpCodes.Ldarg_0) { argIndex = 0; }
            else if (code == OpCodes.Ldarg_1) { argIndex = 1; }
            else if (code == OpCodes.Ldarg_2) { argIndex = 2; }
            else if (code == OpCodes.Ldarg_3) { argIndex = 3; }
            else //if (code == OpCodes.Ldarg_S)
            {
                if (val is not int newArgInd)
                {
                    DoErrorDirect($"Invalid Ldarg_s val {val} - expected int, but got {val?.GetType()?.FullName}");
                    argIndex = 0;
                }
                else
                {
                    argIndex = newArgInd;
                }
            }
            if (Parameters.Length <= argIndex)
            {
                DoErrorDirect($"Invalid Load Arg (code {code} val {val}) - tried to load parameter index {argIndex} but only have {Parameters.Length} params");
            }
            Type param = Parameters[argIndex];
            StackPush(param);
        }
        else if (code == OpCodes.Ldelem || code == OpCodes.Ldelem_I || code == OpCodes.Ldelem_I1 || code == OpCodes.Ldelem_I2 || code == OpCodes.Ldelem_I4 || code == OpCodes.Ldelem_I8
             || code == OpCodes.Ldelem_R4 || code == OpCodes.Ldelem_R8 || code == OpCodes.Ldelem_Ref || code == OpCodes.Ldelem_U1 || code == OpCodes.Ldelem_U2 || code == OpCodes.Ldelem_U4)
        {
            ValidateStackSizeIsAtLeast($"{code}", 2);
            Type index = StackTypes.Pop();
            Type arr = StackTypes.Pop();
            StackSizeChange(0);
            if (index != typeof(int))
            {
                DoErrorDirect($"Invalid {code} index type {index.FullName} - not an int32");
            }
            if (!arr.IsArray)
            {
                DoErrorDirect($"Invalid {code} array type {arr.FullName} - not an array");
            }
            StackPush(arr.GetElementType());
        }
        else if (code == OpCodes.Add || code == OpCodes.Sub || code == OpCodes.And || code == OpCodes.Or || code == OpCodes.Xor || code == OpCodes.Mul || code == OpCodes.Div || code == OpCodes.Div_Un)
        {
            ValidateStackSizeIsAtLeast($"{code}", 2);
            Type a1 = StackTypes.Pop();
            Type a2 = StackTypes.Pop();
            StackSizeChange(0);
            if (a1 != a2)
            {
                DoErrorDirect($"Invalid {code} types - got {a1?.FullName} and {a2?.FullName} - should be same");
            }
            StackPush(a1);
        }
        else if (code == OpCodes.Ldloc || code == OpCodes.Ldloca || code == OpCodes.Ldloc_0 || code == OpCodes.Ldloc_1 || code == OpCodes.Ldloc_2 || code == OpCodes.Ldloc_3 || code == OpCodes.Ldloc_S)
        {
            int locIndex;
            if (code == OpCodes.Ldloc_0) { locIndex = 0; }
            else if (code == OpCodes.Ldloc_1) { locIndex = 1; }
            else if (code == OpCodes.Ldloc_2) { locIndex = 2; }
            else if (code == OpCodes.Ldloc_3) { locIndex = 3; }
            else
            {
                if (val is int newLocInd)
                {
                    locIndex = newLocInd;
                }
                else if (val is LocalBuilder locBuild)
                {
                    locIndex = locBuild.LocalIndex;
                }
                else
                {
                    DoErrorDirect($"Invalid {code} val {val} - expected int or LocalBuilder, but got {val?.GetType()?.FullName}");
                    locIndex = 0;
                }
            }
            if (!Locals.TryGetValue(locIndex, out LocalBuilder local))
            {
                DoErrorDirect($"Invalid {code} index {locIndex} - index not a declared local");
            }
            StackPush(local.LocalType);
        }
        else if (code == OpCodes.Castclass)
        {
            ValidateStackSizeIsAtLeast($"{code}", 1);
            StackSizeChange(-1);
            if (val is not Type newType)
            {
                DoErrorDirect($"Invalid {code} val {val} - expected type but got {val?.GetType()?.FullName}");
            }
            else
            {
                StackPush(newType);
            }
        }
        else if (code == OpCodes.Leave || code == OpCodes.Leave_S)
        {
            ValidateStackSizeIs("Leaving exception block", 0);
        }
        else if (code == OpCodes.Box || code == OpCodes.Unbox_Any)
        {
            if (val is not Type)
            {
                DoErrorDirect($"Invalid Box op (code {code}, to object {val}) - not a Type ({val}={val?.GetType()?.FullName})");
            }
            ValidateStackSizeIsAtLeast($"{code}", 1);
        }
        else
        {
            switch (code.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    break;
                case StackBehaviour.Varpop:
                case StackBehaviour.Pop1:
                case StackBehaviour.Popref:
                case StackBehaviour.Popi:
                    ValidateStackSizeIsAtLeast("opcode " + code, 1);
                    StackSizeChange(-1);
                    break;
                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    ValidateStackSizeIsAtLeast("opcode " + code, 2);
                    StackSizeChange(-2);
                    break;
                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                case StackBehaviour.Popref_popi_pop1:
                    ValidateStackSizeIsAtLeast("opcode " + code, 3);
                    StackSizeChange(-3);
                    break;
            }
            Type pushType = val switch
            {
                null => null,
                MethodInfo method => method.ReturnType,
                FieldInfo field => field.FieldType,
                LocalBuilder local => local.LocalType,
                _ => val.GetType()
            };
            switch (code.StackBehaviourPush)
            {
                case StackBehaviour.Push0:
                    break;
                case StackBehaviour.Push1:
                case StackBehaviour.Pushref:
                case StackBehaviour.Varpush:
                    if (pushType is null)
                    {
                        DoErrorDirect($"Invalid push type for x1 code '{code}' val {val} = {val?.GetType()?.FullName}");
                    }
                    StackPush(pushType);
                    break;
                case StackBehaviour.Pushi:
                    StackPush(typeof(int));
                    break;
                case StackBehaviour.Pushi8:
                    StackPush(typeof(long));
                    break;
                case StackBehaviour.Pushr4:
                    StackPush(typeof(float));
                    break;
                case StackBehaviour.Pushr8:
                    StackPush(typeof(double));
                    break;
                case StackBehaviour.Push1_push1:
                    if (pushType is null)
                    {
                        DoErrorDirect($"Invalid push type for x2 code '{code}' val {val} = {val?.GetType()?.FullName}");
                    }
                    StackPush(pushType);
                    StackPush(pushType);
                    break;
            }
        }
        ValidateStackSizeIsAtLeast("post opcode " + code, 0);
#endif
    }

    /// <summary>Defines a label.</summary>
    /// <returns>The label.</returns>
    public Label DefineLabel()
    {
        return Internal.DefineLabel();
    }

    /// <summary>
    /// Starts a filtered 'try' block.
    /// Usage pattern:
    /// <code>
    /// Label exceptionLabel = BeginExceptionBlock();
    /// ... risky code ...
    /// Emit(OpCodes.Leave, exceptionLabel);
    /// BeginCatchBlock(typeof(Exception));
    /// ... catch code ...
    /// EndExceptionBlock();
    /// </code>
    /// </summary>
    /// <returns>The block label.</returns>
    public Label BeginExceptionBlock()
    {
        Label toRet = Internal.BeginExceptionBlock();
        AddCode(OpCodes.Nop, toRet, "<start try block, label>");
        ValidateStackSizeIs("Starting exception block", 0);
        return toRet;
    }

    /// <summary>Changes the stack size.</summary>
    /// <param name="amount">The amount to change by.</param>
    [Conditional("VALIDATE")]
    public void StackSizeChange(int amount)
    {
#if VALIDATE
        if (amount == 0)
        {
            AddCode(OpCodes.Nop, $"<stack size moved out-of-track, now: {StackTypes.Count}>", "minor");
            return;
        }
        while (amount < 0 && StackTypes.Any())
        {
            StackTypes.Pop();
            amount++;
        }
        AddCode(OpCodes.Nop, $"<stack size move: {amount}, now: {StackTypes.Count}>", "minor");
#endif
    }

    /// <summary>Adds to the type stack.</summary>
    /// <param name="type">The new type.</param>
    [Conditional("VALIDATE")]
    public void StackPush(Type type)
    {
#if VALIDATE
        StackTypes.Push(type);
        AddCode(OpCodes.Nop, $"<stack size move: +1, added {type?.FullName}, now: {StackTypes.Count}>", "minor");
#endif
    }

    /// <summary>Starts a catch block for a specific exception type.</summary>
    public void BeginCatchBlock(Type exType)
    {
        Internal.BeginCatchBlock(exType);
        AddCode(OpCodes.Nop, exType, "<begin catch block, type>");
        ValidateStackSizeIs("Starting catch block", 0);
        StackSizeChange(1);
    }

    /// <summary>Begins a finally block.</summary>
    public void BeginFinallyBlock()
    {
        Internal.BeginFinallyBlock();
        AddCode(OpCodes.Nop, null, "<BeginFinallyBlock>");
        ValidateStackSizeIs("Beginning finally block", 0);
    }

    /// <summary>Ends an exception block.</summary>
    public void EndExceptionBlock()
    {
        Internal.EndExceptionBlock();
        AddCode(OpCodes.Nop, null, "<EndExceptionBlock>");
        ValidateStackSizeIs("Ending exception block", 0);
    }

    /// <summary>Marks a label.</summary>
    /// <param name="label">The label.</param>
    public void MarkLabel(Label label)
    {
        Internal.MarkLabel(label);
        AddCode(OpCodes.Nop, label.GetHashCode(), "<MarkLabel>");
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    public void Emit(OpCode code)
    {
        Internal.Emit(code);
        AddCode(code, null);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, FieldInfo dat)
    {
        Internal.Emit(code, dat);
        AddCode(OpCodes.Nop, $"{dat.Name} <{dat.FieldType.FullName}>", $"<{code}>");
        Validator(code, dat);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="t">The associated data.</param>
    public void Emit(OpCode code, Type t)
    {
        Internal.Emit(code, t);
        AddCode(code, t);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    /// <param name="altParams">The number of parameters, if GetParameters is not stable.</param>
    public void Emit(OpCode code, ConstructorInfo dat, int? altParams = null)
    {
        Internal.Emit(code, dat);
        AddCode(OpCodes.Nop, $"{BaseCode}{dat.DeclaringType.Name} constructor: {dat}", code.ToString().ToLowerFast());
        Validator(code, dat, altParams);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, Label[] dat)
    {
        Internal.Emit(code, dat);
        AddCode(code, dat);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    /// <param name="altParams">The number of parameters, if GetParameters is not stable.</param>
    public void Emit(OpCode code, MethodInfo dat, int? altParams = null)
    {
        Internal.Emit(code, dat);
        AddCode(OpCodes.Nop, $"{BaseCode}{dat}: {dat.DeclaringType?.Name}", code.ToString().ToLowerFast());
        Validator(code, dat, altParams);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, Label dat)
    {
        Internal.Emit(code, dat);
        AddCode(code, dat.GetHashCode());
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, string dat)
    {
        Internal.Emit(code, dat);
        AddCode(code, dat);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, int dat)
    {
        Internal.Emit(code, dat);
        AddCode(code, dat);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, long dat)
    {
        Internal.Emit(code, dat);
        AddCode(code, dat);
    }

    /// <summary>Emits an operation.</summary>
    /// <param name="code">The operation code.</param>
    /// <param name="dat">The associated data.</param>
    public void Emit(OpCode code, LocalBuilder dat)
    {
        Internal.Emit(code, dat);
        AddCode(OpCodes.Nop, $"{BaseCode}{dat.LocalIndex} <{dat.LocalType.FullName}>", $"<{code}>");
        Validator(code, dat);
    }

    /// <summary>Declares a local.</summary>
    /// <param name="t">The type.</param>
    public LocalBuilder DeclareLocal(Type t)
    {
        LocalBuilder x = Internal.DeclareLocal(t);
        AddCode(OpCodes.Nop, $"{t.FullName} as {x.LocalIndex}", "<declare local>");
#if VALIDATE
        Locals.Add(x.LocalIndex, x);
#endif
        return x;
    }

    /// <summary>Adds a comment to the developer debug of the IL output.</summary>
    /// <param name="str">The comment text.</param>
    [Conditional("VALIDATE")]
    public void Comment(string str)
    {
        AddCode(OpCodes.Nop, $"-- {EmphasizeCode}{str}{BaseCode} --", "// Comment");
    }

    /// <summary>Adds a comment to the developer debug of the IL output showing the full current stack.</summary>
    [Conditional("VALIDATE")]
    public void CommentStack()
    {
#if VALIDATE
        AddCode(OpCodes.Nop, string.Join(", ", StackTypes.Select(t => t.FullName)), "// Stack Report");
#endif
    }

    /// <summary>Emits a <see cref="Console.WriteLine(string?)"/> directly (for debugging usage mainly).</summary>
    public void EmitWriteLine(string val)
    {
        Internal.EmitWriteLine(val);
    }

    /// <summary>Emits a <see cref="Console.WriteLine(object)"/> directly with a local variable (for debugging usage mainly).</summary>
    public void EmitWriteLine(LocalBuilder loc)
    {
        Internal.EmitWriteLine(loc);
    }
}
#pragma warning restore CA1822 // Mark members as static
