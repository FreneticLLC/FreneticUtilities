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

namespace FreneticUtilities.FreneticToolkit
{
    /// <summary>Tracks generated IL.</summary>
    public class ILGeneratorTracker
    {

        /// <summary>Internal generator.</summary>
        public ILGenerator Internal;

        /// <summary>Optional text format codes.</summary>
        public string EmphasizeCode = "", BaseCode = "", MinorCode = "";

#if DEBUG
        /// <summary>All codes generated. Only has a value when compiled in DEBUG mode.</summary>
        public List<KeyValuePair<string, object>> Codes = new();
#endif

        /// <summary>Stack size tracker, for validation.</summary>
        public int StackSize = 0;

        /// <summary>Gives a warning if stack size is not the exact correct size.</summary>
        /// <param name="situation">The current place in the code that requires a validation.</param>
        /// <param name="expected">The expected stack size.</param>
        [Conditional("VALIDATE")]
        public void ValidateStackSizeIs(string situation, int expected)
        {
            if (StackSize != expected)
            {
                Error($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackSize}{BaseCode}"
                    + $" but should be exactly {EmphasizeCode}{expected}{BaseCode} for code:\n{MinorCode}{Stringify()}");
            }
        }

        /// <summary>Gives a warning if stack size is not at least the given size.</summary>
        /// <param name="situation">The current place in the code that requires a validation.</param>
        /// <param name="expected">The expected minimum stack size.</param>
        [Conditional("VALIDATE")]
        public void ValidateStackSizeIsAtLeast(string situation, int expected)
        {
            if (StackSize < expected)
            {
                Error($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackSize}{BaseCode}"
                    + $" but should be at least {EmphasizeCode}{expected}{BaseCode} for code:\n{MinorCode}{Stringify()}");
            }
        }

        /// <summary>Gives a warning if stack size is not at most the given size.</summary>
        /// <param name="situation">The current place in the code that requires a validation.</param>
        /// <param name="expected">The expected maxium stack size.</param>
        [Conditional("VALIDATE")]
        public void ValidateStackSizeIsAtMost(string situation, int expected)
        {
            if (StackSize > expected)
            {
                Error($"Stack not well sized at {EmphasizeCode}{situation}{BaseCode}... size = {EmphasizeCode}{StackSize}{BaseCode}"
                    + $" but should be at most {EmphasizeCode}{expected}{BaseCode} for code:\n{MinorCode}{Stringify()}");
            }
        }

        /// <summary>Creates a string of all the generated CIL code.</summary>
        /// <returns>Generated CIL code string.</returns>
#pragma warning disable CA1822 // Mark members as static
        public string Stringify()
#pragma warning restore CA1822 // Mark members as static
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
        /// When compiled in DEBUG mode, adds a code value to the <see cref="Codes"/> list.
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
        /// <param name="altParams">The number of parameters if GetParameters() is not stable.</param>
        [Conditional("VALIDATE")]
        public void Validator(OpCode code, object val, int? altParams = null)
        {
            if (code == OpCodes.Nop)
            {
                // Do nothing
            }
            else if (code == OpCodes.Ret)
            {
                ValidateStackSizeIsAtMost("return op", 1);
                StackSize = 0;
            }
            else if (code == OpCodes.Call || code == OpCodes.Callvirt)
            {
                if (val is not MethodInfo method)
                {
                    Console.WriteLine("Invalid call (code " + code + ", to object " + val + ") - not a method reference");
                }
                else
                {
                    int paramCount = altParams ?? method.GetParameters().Length;
                    if (!method.IsStatic)
                    {
                        paramCount++;
                    }
                    ValidateStackSizeIsAtLeast("call opcode " + code, paramCount);
                    StackSizeChange(-paramCount);
                    if (method.ReturnType != typeof(void))
                    {
                        StackSizeChange(1);
                    }
                }
            }
            else if (code == OpCodes.Newobj)
            {
                if (val is not ConstructorInfo method)
                {
                    Console.WriteLine("Invalid NEWOBJ (code " + code + ", to object " + val + ") - not a constructor reference");
                }
                else
                {
                    int paramCount = altParams ?? method.GetParameters().Length;
                    ValidateStackSizeIsAtLeast("opcode newobj", paramCount);
                    StackSizeChange(-paramCount);
                    StackSizeChange(1);
                }
            }
            else if (code == OpCodes.Leave || code == OpCodes.Leave_S)
            {
                ValidateStackSizeIs("Leaving exception block", 0);
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
                switch (code.StackBehaviourPush)
                {
                    case StackBehaviour.Push0:
                        break;
                    case StackBehaviour.Push1:
                    case StackBehaviour.Pushi:
                    case StackBehaviour.Pushi8:
                    case StackBehaviour.Pushr4:
                    case StackBehaviour.Pushr8:
                    case StackBehaviour.Pushref:
                    case StackBehaviour.Varpush:
                        StackSizeChange(1);
                        break;
                    case StackBehaviour.Push1_push1:
                        StackSizeChange(2);
                        break;
                }
            }
            ValidateStackSizeIsAtLeast("post opcode " + code, 0);
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
            StackSize += amount;
            AddCode(OpCodes.Nop, "<stack size move: " + amount + ", now: " + StackSize + ">", "minor");
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
            AddCode(code, dat.Name + " <" + dat.FieldType.Name + ">");
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
            AddCode(OpCodes.Nop, dat.DeclaringType.Name + " constructor: " + dat, code.ToString().ToLowerFast());
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
            AddCode(OpCodes.Nop, dat + ": " + dat.DeclaringType?.Name, code.ToString().ToLowerFast());
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

        /// <summary>Declares a local.</summary>
        /// <param name="t">The type.</param>
        public int DeclareLocal(Type t)
        {
            int x = Internal.DeclareLocal(t).LocalIndex;
            AddCode(OpCodes.Nop, t.FullName + " as " + x, "<declare local>");
            return x;
        }

        /// <summary>Adds a comment to the developer debug of the IL output.</summary>
        /// <param name="str">The comment text.</param>
        public void Comment(string str)
        {
            AddCode(OpCodes.Nop, "-- " + str + " --", "// Comment");
        }
    }
}
