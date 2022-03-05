//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

#if DEBUG
#define VALIDATE
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>Internal utility for <see cref="AutoConfiguration"/> to do dynamic code generation.</summary>
    public static class AutoConfigurationCodeGenerator
    {
        /// <summary>Helper class that represents the tools needed to convert <see cref="FDSData"/> to the final output type.</summary>
        public class DataConverter
        {
            /// <summary>Method that gets the data from an <see cref="FDSData"/> instance.</summary>
            public MethodInfo Getter;

            /// <summary>Method that gets the value of the data from a nullable instance (if needed).</summary>
            public MethodInfo ValueGrabber;
        }

        /// <summary>Static map of C# class type to the internal executable data needed to process it.</summary>
        public static ConcurrentDictionary<Type, AutoConfiguration.Internal.AutoConfigData> TypeMap = new();

        /// <summary>Locker for generating new config data.</summary>
        public static LockObject GenerationLock = new();

        /// <summary>A 1-value type array, with the value being <see cref="AutoConfiguration"/>.</summary>
        public static Type[] AutoConfigArray = new Type[] { typeof(AutoConfiguration) };

        /// <summary>Array of types for input to <see cref="AutoConfiguration.Internal.AutoConfigData.SaveSection"/>.</summary>
        public static Type[] SaveMethodInputTypeArray = new Type[] { typeof(AutoConfiguration), typeof(bool) };

        /// <summary>Array of types for input to <see cref="AutoConfiguration.Internal.AutoConfigData.LoadSection"/>.</summary>
        public static Type[] LoadMethodInputTypeArray = new Type[] { typeof(AutoConfiguration), typeof(FDSSection) };

        /// <summary>A reference to the <see cref="AutoConfiguration.Save"/> method.</summary>
        public static MethodInfo ConfigSaveMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Save));

        /// <summary>A reference to the <see cref="AutoConfiguration.Load"/> method.</summary>
        public static MethodInfo ConfigLoadMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Load));

        /// <summary>A reference to the <see cref="FDSSection.SetRootData(string, FDSData)"/> method.</summary>
        public static MethodInfo SectionSetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.SetRootData), new Type[] { typeof(string), typeof(FDSData) });

        /// <summary>A reference to the <see cref="FDSSection.GetSection(string)"/> method.</summary>
        public static MethodInfo SectionGetSectionMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetSection), new Type[] { typeof(string) });

        /// <summary>A reference to the <see cref="FDSSection.GetRootData(string)"/> method.</summary>
        public static MethodInfo SectionGetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetRootData), new Type[] { typeof(string) });

        /// <summary>A reference to the <see cref="FDSSection.GetRootKeys"/> method.</summary>
        public static MethodInfo SectionGetRootKeysMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetRootKeys));

        /// <summary>A reference to the <see cref="FixNull{T}(T?)"/> method.</summary>
        public static MethodInfo FixNullMethod = typeof(AutoConfigurationCodeGenerator).GetMethod(nameof(FixNull));

        /// <summary>A reference to the <see cref="List{FDSData}.Add"/> method for lists of <see cref="FDSData"/>.</summary>
        public static MethodInfo FDSDataListAddMethod = typeof(List<FDSData>).GetMethod(nameof(List<FDSData>.Add));

        /// <summary>A reference to the <see cref="List{FDSData}.GetEnumerator"/> method for lists of <see cref="FDSData"/>.</summary>
        public static MethodInfo FDSDataListGetEnumeratorMethod = typeof(List<FDSData>).GetMethod(nameof(List<FDSData>.GetEnumerator));

        /// <summary>A reference to the <see cref="List{FDSData}.Enumerator.MoveNext"/> method for a list of <see cref="FDSData"/>.</summary>
        public static MethodInfo FDSDataListEnumeratorMoveNextMethod = typeof(List<FDSData>.Enumerator).GetMethod(nameof(List<FDSData>.Enumerator.MoveNext));

        /// <summary>A reference to the <see cref="IDisposable.Dispose"/> method.</summary>
        public static MethodInfo IDisposableDisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));

        /// <summary>A reference to the <see cref="List{FDSData}.Enumerator.Current"/> property getter for a list of <see cref="FDSData"/>.</summary>
        public static MethodInfo FDSDataListEnumeratorCurrentGetter = typeof(List<FDSData>.Enumerator).GetProperty(nameof(List<FDSData>.Enumerator.Current)).GetMethod;

        /// <summary>A reference to the <see cref="FDSData.AsDataList"/> property getter method.</summary>
        public static MethodInfo FDSDataAsDataListGetter = typeof(FDSData).GetProperty(nameof(FDSData.AsDataList)).GetMethod;

        /// <summary>A reference to the <see cref="FDSData.Internal"/> field.</summary>
        public static FieldInfo FDSDataInternalField = typeof(FDSData).GetField(nameof(FDSData.Internal));

        /// <summary>A reference to the <see cref="FDSSection"/> no-arguments constructor.</summary>
        public static ConstructorInfo SectionConstructor = typeof(FDSSection).GetConstructor(Array.Empty<Type>());

        /// <summary>A reference to the <see cref="FDSData"/> one-argument constructor.</summary>
        public static ConstructorInfo FDSDataObjectConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object) });

        /// <summary>A reference to the <see cref="FDSData"/> two-arguments constructor.</summary>
        public static ConstructorInfo FDSDataObjectCommentConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object), typeof(string) });

        /// <summary>A reference to the <see cref="List{FDSData}"/> of <see cref="FDSData"/> no-arguments constructor.</summary>
        public static ConstructorInfo FDSDataListConstructor = typeof(List<FDSData>).GetConstructor(Array.Empty<Type>());

        /// <summary>A reference to <see cref="AutoConfiguration.InternalData"/>.</summary>
        public static FieldInfo AutoConfigurationInternalDataField = typeof(AutoConfiguration).GetField(nameof(AutoConfiguration.InternalData));

        /// <summary>A reference to <see cref="AutoConfiguration.LocalInternal.IsFieldModified"/>.</summary>
        public static FieldInfo AutoConfigurationModifiedArrayField = typeof(AutoConfiguration.LocalInternal).GetField(nameof(AutoConfiguration.LocalInternal.IsFieldModified));

        /// <summary>A reference to the <see cref="IEnumerable{T}.GetEnumerator"/> method with type <see cref="string"/>.</summary>
        public static MethodInfo IEnumerableStringGetEnumeratorMethod = typeof(IEnumerable<string>).GetMethod(nameof(IEnumerable<string>.GetEnumerator));

        /// <summary>A reference to the <see cref="IEnumerator.MoveNext"/> method.</summary>
        public static MethodInfo IEnumeratorMoveNextMethod = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));

        /// <summary>A reference to the <see cref="IEnumerator.Current"/> property getter.</summary>
        public static MethodInfo IEnumeratorCurrentGetter = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetMethod;

        /// <summary>A reference to the <see cref="object.ToString"/> method.</summary>
        public static MethodInfo ObjectToStringMethod = typeof(object).GetMethod(nameof(object.ToString));

        /// <summary>A mapping of core object types to the method that converts <see cref="FDSData"/> to them.</summary>
        public static Dictionary<Type, DataConverter> FDSDataFieldsByType = new(64);

        /// <summary>Init required static data.</summary>
        static AutoConfigurationCodeGenerator()
        {
            static void register(Type type, string propertyName)
            {
                DataConverter converter = new()
                {
                    Getter = typeof(FDSData).GetProperty(propertyName).GetMethod,
                };
                if (Nullable.GetUnderlyingType(converter.Getter.ReturnType) != null)
                {
                    converter.ValueGrabber = FixNullMethod.MakeGenericMethod(type);
                }
                FDSDataFieldsByType[type] = converter;
            }
            register(typeof(bool), nameof(FDSData.AsBool));
            register(typeof(long), nameof(FDSData.AsLong));
            register(typeof(int), nameof(FDSData.AsInt));
            register(typeof(short), nameof(FDSData.AsShort));
            register(typeof(sbyte), nameof(FDSData.AsSByte));
            register(typeof(ulong), nameof(FDSData.AsULong));
            register(typeof(uint), nameof(FDSData.AsUInt));
            register(typeof(ushort), nameof(FDSData.AsUShort));
            register(typeof(byte), nameof(FDSData.AsByte));
            register(typeof(decimal), nameof(FDSData.AsDecimal));
            register(typeof(double), nameof(FDSData.AsDouble));
            register(typeof(float), nameof(FDSData.AsFloat));
            register(typeof(string), nameof(FDSData.AsString));
            register(typeof(byte[]), nameof(FDSData.AsByteArray));
        }

        /// <summary>Utility method to perform "Nullable<typeparamref name="T"/>.Value" since CIL is very bad at emitting this properly.</summary>
        /// <typeparam name="T">The ValueType that will be Nullable.</typeparam>
        /// <param name="val">The instance to convert.</param>
        /// <returns>The non-nullable value.</returns>
        public static T FixNull<T>(T? val) where T : struct
        {
            return val.Value;
        }

        /// <summary>A mapping of types to methods that load a list of that type from a <see cref="List{T}"/> of <see cref="FDSData"/>.</summary>
        public static Dictionary<Type, DynamicMethod> ListLoaders = new(32);

        /// <summary>A mapping of types to methods that save a list of that type to a <see cref="List{T}"/> of <see cref="FDSData"/>.</summary>
        public static Dictionary<Type, DynamicMethod> ListSavers = new(32);

        /// <summary>
        /// Gets or creates a method that loads a list of the specified type from a <see cref="IDictionary{TKey, TValue}"/> of <see cref="FDSData"/>.
        /// Uses <see cref="ListLoaders"/> as a backing map.
        /// </summary>
        /// <param name="type">The Dictionary type to load to, like typeof 'Dictionary&lt;int, string&gt;'.</param>
        /// <returns>The method that loads to it.</returns>
        public static DynamicMethod GetDictionaryLoader(Type type)
        {
            if (ListLoaders.TryGetValue(type, out DynamicMethod method))
            {
                return method;
            }
            DynamicMethod generated = CreateDictionaryConverter(type, true);
            ListLoaders[type] = generated;
            return generated;
        }

        /// <summary>
        /// Gets or creates a method that saves a list of the specified type to a <see cref="IDictionary{TKey, TValue}"/> of <see cref="FDSData"/>.
        /// Uses <see cref="ListSavers"/> as a backing map.
        /// </summary>
        /// <param name="type">The Dictionary type to save from, like typeof 'Dictionary&lt;int, string&gt;'.</param>
        /// <returns>The method that saves from it.</returns>
        public static DynamicMethod GetDictionarySaver(Type type)
        {
            if (ListSavers.TryGetValue(type, out DynamicMethod method))
            {
                return method;
            }
            DynamicMethod generated = CreateDictionaryConverter(type, false);
            ListSavers[type] = generated;
            return generated;
        }

        /// <summary>
        /// Gets or creates a method that loads a list of the specified type from a <see cref="List{T}"/> of <see cref="FDSData"/>.
        /// Uses <see cref="ListLoaders"/> as a backing map.
        /// </summary>
        /// <param name="type">The list type to load to, like typeof 'List&lt;int&gt;'.</param>
        /// <returns>The method that loads to it.</returns>
        public static DynamicMethod GetListLoader(Type type)
        {
            if (ListLoaders.TryGetValue(type, out DynamicMethod method))
            {
                return method;
            }
            DynamicMethod generated = CreateListConverter(type, true);
            ListLoaders[type] = generated;
            return generated;
        }

        /// <summary>
        /// Gets or creates a method that saves a list of the specified type to a <see cref="List{T}"/> of <see cref="FDSData"/>.
        /// Uses <see cref="ListSavers"/> as a backing map.
        /// </summary>
        /// <param name="type">The list type to save from, like typeof 'List&lt;int&gt;'.</param>
        /// <returns>The method that saves from it.</returns>
        public static DynamicMethod GetListSaver(Type type)
        {
            if (ListSavers.TryGetValue(type, out DynamicMethod method))
            {
                return method;
            }
            DynamicMethod generated = CreateListConverter(type, false);
            ListSavers[type] = generated;
            return generated;
        }

        /// <summary>Generates a method that loads a list of the specified type from a <see cref="List{T}"/> of <see cref="FDSData"/>.</summary>
        /// <param name="type">The type to convert to/from.</param>
        /// <param name="doLoad">True indicates load, false indicates save.</param>
        /// <returns>The generated method.</returns>
        public static DynamicMethod CreateListConverter(Type type, bool doLoad)
        {
            Type listSubType = type.GetGenericArguments()[0];
            Type inListType, outListType, inSubType, outSubType, enumeratorType;
            LocalBuilder outListVariable, inListVariable;
            MethodInfo enumeratorMethod, enumeratorMoveNextMethod, enumeratorCurrentGetter, listAddMethod;
            ConstructorInfo outListConstructor;
            if (doLoad)
            {
                inListType = typeof(List<FDSData>);
                outListType = type;
                inSubType = typeof(FDSData);
                outSubType = listSubType;
                enumeratorMethod = FDSDataListGetEnumeratorMethod;
                enumeratorType = typeof(List<FDSData>.Enumerator);
                enumeratorMoveNextMethod = FDSDataListEnumeratorMoveNextMethod;
                enumeratorCurrentGetter = FDSDataListEnumeratorCurrentGetter;
                listAddMethod = outListType.GetMethod(nameof(ICollection<int>.Add));
                outListConstructor = type.GetConstructor(Array.Empty<Type>());
            }
            else
            {
                inListType = type;
                outListType = typeof(List<FDSData>);
                inSubType = listSubType;
                outSubType = typeof(FDSData);
                enumeratorMethod = inListType.GetMethod(nameof(ICollection.GetEnumerator));
                enumeratorType = enumeratorMethod.ReturnType;
                enumeratorMoveNextMethod = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext));
                enumeratorCurrentGetter = enumeratorType.GetProperty(nameof(IEnumerator.Current)).GetMethod;
                listAddMethod = FDSDataListAddMethod;
                outListConstructor = FDSDataListConstructor;
            }
            DynamicMethod genMethod = new("ListConvert", outListType, new Type[] { inListType }, typeof(AutoConfiguration).Module, true);
            ILGeneratorTracker targetILGen = new(genMethod.GetILGenerator(), genMethod, $"ListConvert_{inListType.Name}");
            targetILGen.Emit(OpCodes.Ldarg_0); // Load the input parameter (stack=paramInList)
            // Store input list and new output list to local variable
            inListVariable = targetILGen.DeclareLocal(inListType); // List<TIn> inList;
            targetILGen.Emit(OpCodes.Stloc, inListVariable); // inList = dataList; (stack now clear)
            targetILGen.Emit(OpCodes.Newobj, outListConstructor); // new List<TOut>(); (stack=newOutList)
            outListVariable = targetILGen.DeclareLocal(outListType); // List<TOut> outList;
            targetILGen.Emit(OpCodes.Stloc, outListVariable); // outList = newOutList; (stack now clear)
            // foreach (TIn datum in inList)
            // Gather enumerator data
            targetILGen.Emit(OpCodes.Ldloc, inListVariable); // Load the inList (stack=inList)
            targetILGen.Emit(OpCodes.Call, enumeratorMethod); // call inList.GetEnumerator() (stack=gottenEnum)
            LocalBuilder enumeratorVariable = targetILGen.DeclareLocal(enumeratorType);
            targetILGen.Emit(OpCodes.Stloc, enumeratorVariable); // Enumerator<TIn> enumerator = gottenEnum; (stack now clear)
            Label tryBlock = targetILGen.BeginExceptionBlock(); // try { // for the 'finally' block farther down
            Label loopCheck = targetILGen.DefineLabel();
            targetILGen.Emit(OpCodes.Br, loopCheck); // Jump to the loop check first
            // Loop body
            Label blockStart = targetILGen.DefineLabel();
            targetILGen.MarkLabel(blockStart);
            targetILGen.Emit(OpCodes.Ldloc, outListVariable); // Load the outlist (stack=outList)
            targetILGen.Emit(OpCodes.Ldloca, enumeratorVariable); // Load the enumerator (stack=outList,enumerator)
            targetILGen.Emit(OpCodes.Call, enumeratorCurrentGetter); // Call TIn enumerator.Current getter (stack=outList,datum)
            EmitTypeConverter(listSubType, targetILGen, doLoad); // Convert the data as-needed
            if (!doLoad)
            {
                targetILGen.Emit(OpCodes.Newobj, FDSDataObjectConstructor); // new FDSData(out-data)
            }
            targetILGen.Emit(OpCodes.Call, listAddMethod); // call outList.add(datum); (stack now clear)
            // Loop check (MoveNext)
            targetILGen.MarkLabel(loopCheck);
            targetILGen.Emit(OpCodes.Ldloca, enumeratorVariable); // Load the enumerator (stack=enumerator)
            targetILGen.Emit(OpCodes.Call, enumeratorMoveNextMethod); // Call bool enumator.MoveNext() (stack=hasNext)
            targetILGen.Emit(OpCodes.Brtrue, blockStart); // If true, jump to block start
            // else, continue to finally logic
            // Finally block
            targetILGen.Emit(OpCodes.Leave, tryBlock); // end the 'try' block
            targetILGen.BeginFinallyBlock(); // finally {
            targetILGen.Emit(OpCodes.Ldloca, enumeratorVariable); // load enumerator variable (stack=enumerator)
            targetILGen.Emit(OpCodes.Constrained, enumeratorType); // constrain the type for the disposable call
            targetILGen.Emit(OpCodes.Callvirt, IDisposableDisposeMethod); // enumerator.Dispose(); (stack now clear)
            targetILGen.EndExceptionBlock(); // end the finally { block
            // End of method (return)
            targetILGen.Emit(OpCodes.Ldloc, outListVariable); // load the outList onto stack (stack=outList)
            targetILGen.Emit(OpCodes.Ret); // return outList;
            return genMethod;
        }

        /// <summary>Generates a method that loads a Dictionary of the specified type from a <see cref="IDictionary{TKey, TValue}"/>.</summary>
        /// <param name="type">The type to convert to/from.</param>
        /// <param name="doLoad">True indicates load, false indicates save.</param>
        /// <returns>The generated method.</returns>
        public static DynamicMethod CreateDictionaryConverter(Type type, bool doLoad)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];
            Type outType, inType, enumeratorType, outKeyType, actualValueType, inKeyType;
            ConstructorInfo outConstructor;
            MethodInfo enumeratorMethod, getKeysMethod, addMethod, getValueMethod, enumeratorMoveNextMethod, enumeratorCurrentGetter;
            if (doLoad)
            {
                inType = typeof(FDSSection);
                outType = type;
                outKeyType = keyType;
                inKeyType = typeof(string);
                actualValueType = valueType;
                outConstructor = type.GetConstructor(Array.Empty<Type>());
                getKeysMethod = SectionGetRootKeysMethod;
                enumeratorMethod = IEnumerableStringGetEnumeratorMethod;
                enumeratorType = typeof(IEnumerator<string>);
                addMethod = type.GetMethod(nameof(IDictionary.Add), type.GetGenericArguments());
                getValueMethod = SectionGetRootDataMethod;
                enumeratorMoveNextMethod = IEnumeratorMoveNextMethod;
                enumeratorCurrentGetter = IEnumeratorCurrentGetter;
            }
            else
            {
                inType = type;
                outType = typeof(FDSSection);
                outKeyType = typeof(string);
                inKeyType = keyType;
                actualValueType = valueType;
                outConstructor = SectionConstructor;
                getKeysMethod = type.GetProperty(nameof(IDictionary.Keys)).GetMethod;
                enumeratorMethod = getKeysMethod.ReturnType.GetMethod(nameof(ICollection.GetEnumerator));
                enumeratorType = enumeratorMethod.ReturnType;
                enumeratorMoveNextMethod = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext));
                enumeratorCurrentGetter = enumeratorType.GetProperty(nameof(IEnumerator.Current)).GetMethod;
                addMethod = SectionSetRootDataMethod;
                getValueMethod = type.GetMethod("get_Item");
            }
            DynamicMethod genMethod = new("DictionaryConvert", outType, new Type[] { inType }, typeof(AutoConfiguration).Module, true);
            ILGeneratorTracker targetILGen = new(genMethod.GetILGenerator(), genMethod, $"DictionaryConvert_{inType.Name}");
            // Dictionary<TKey, TValue> result = new();
            LocalBuilder resultLocal = targetILGen.DeclareLocal(outType);
            targetILGen.Emit(OpCodes.Newobj, outConstructor);
            targetILGen.Emit(OpCodes.Stloc, resultLocal);
            // IEnumerator<string> keysEnumerator = section.GetRootKeys().GetEnumerator();
            LocalBuilder keysEnumeratorLocal = targetILGen.DeclareLocal(enumeratorType);
            targetILGen.Emit(OpCodes.Ldarg_0);
            targetILGen.Emit(OpCodes.Call, getKeysMethod);
            targetILGen.Emit(OpCodes.Callvirt, enumeratorMethod);
            targetILGen.Emit(OpCodes.Stloc, keysEnumeratorLocal);
            LocalBuilder keyLocal = targetILGen.DeclareLocal(inKeyType); // TKey key;
            // try {
            Label tryBlock = targetILGen.BeginExceptionBlock();
            // 
            Label loopCheck = targetILGen.DefineLabel();
            targetILGen.Emit(OpCodes.Br, loopCheck); // Jump to the loop check first
            // Loop body
            Label blockStart = targetILGen.DefineLabel();
            targetILGen.MarkLabel(blockStart);
            // key = keysEnumerator.Current;
            targetILGen.Emit(enumeratorType.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, keysEnumeratorLocal);
            targetILGen.Emit(enumeratorType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, enumeratorCurrentGetter);
            targetILGen.Emit(OpCodes.Stloc, keyLocal);
            // result.Add(ConvertKey(key), ConvertValue(section.GetRootData(key)));
            targetILGen.Emit(OpCodes.Ldloc, resultLocal);
            targetILGen.Emit(OpCodes.Ldloc, keyLocal);
            if (doLoad)
            {
                targetILGen.Emit(OpCodes.Newobj, FDSDataObjectConstructor); // use FDSData to convert the key type
                EmitTypeConverter(outKeyType, targetILGen, doLoad);
            }
            else
            {
                if (keyLocal.LocalType.IsValueType)
                {
                    targetILGen.Emit(OpCodes.Box, keyLocal.LocalType);
                }
                targetILGen.Emit(OpCodes.Callvirt, ObjectToStringMethod);
            }
            targetILGen.Emit(OpCodes.Ldarg_0);
            targetILGen.Emit(OpCodes.Ldloc, keyLocal);
            targetILGen.Emit(OpCodes.Call, getValueMethod);
            EmitTypeConverter(actualValueType, targetILGen, doLoad);
            if (!doLoad)
            {
                targetILGen.Emit(OpCodes.Newobj, FDSDataObjectConstructor); // new FDSData(out-data)
            }
            targetILGen.Emit(OpCodes.Call, addMethod);
            // Loop check (MoveNext)
            targetILGen.MarkLabel(loopCheck);
            // if (keysEnumerator.MoveNext()) {
            targetILGen.Emit(enumeratorType.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, keysEnumeratorLocal);
            targetILGen.Emit(enumeratorType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, enumeratorMoveNextMethod);
            // goto loopStart;
            targetILGen.Emit(OpCodes.Brtrue, blockStart);
            // } else { goto finally; }
            // } finally {
            targetILGen.Emit(OpCodes.Leave, tryBlock);
            targetILGen.BeginFinallyBlock();
            // keysEnumerator.Dispose();
            if (doLoad)
            {
                targetILGen.Emit(OpCodes.Ldloc, keysEnumeratorLocal);
                targetILGen.Emit(OpCodes.Callvirt, IDisposableDisposeMethod);
            }
            // }
            targetILGen.EndExceptionBlock();
            // return result;
            targetILGen.Emit(OpCodes.Ldloc, resultLocal);
            targetILGen.Emit(OpCodes.Ret);
            return genMethod;
        }

        /// <summary>
        /// Emits the appropriate <see cref="FDSData"/> convert method for the applicable type.
        /// <para>Expected stack condition for load is input one <see cref="FDSData"/> on top of stack at start, output one object of type param <paramref name="type"/> on top of stack at end, and save is reverse of that.</para>
        /// </summary>
        /// <param name="type">The type to convert to/from.</param>
        /// <param name="targetILGen">The IL Generator to emit to.</param>
        /// <param name="doLoad">True indicates load, false indicates save.</param>
        public static void EmitTypeConverter(Type type, ILGeneratorTracker targetILGen, bool doLoad)
        {
            targetILGen.Comment($"Convert value to {type.FullName}");
            if (FDSDataFieldsByType.TryGetValue(type, out DataConverter converter))
            {
                if (doLoad)
                {
                    targetILGen.Emit(OpCodes.Callvirt, converter.Getter); // Call the loader method
                    if (converter.ValueGrabber != null)
                    {
                        targetILGen.Emit(OpCodes.Call, converter.ValueGrabber); // Call the nullable converter if needed
                    }
                }
                else if (!doLoad && type.IsValueType)
                {
                    targetILGen.Emit(OpCodes.Box, type); // Box value types before sending along.
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (doLoad)
                {
                    targetILGen.Emit(OpCodes.Ldfld, FDSDataInternalField);
                    targetILGen.Emit(OpCodes.Castclass, typeof(FDSSection));
                }
                targetILGen.Emit(OpCodes.Call, doLoad ? GetDictionaryLoader(type) : GetDictionarySaver(type)); // Call the relevant Dictionary converter method
            }
            else if (typeof(ICollection).IsAssignableFrom(type))
            {
                if (doLoad)
                {
                    targetILGen.Emit(OpCodes.Call, FDSDataAsDataListGetter);
                }
                targetILGen.Emit(OpCodes.Call, doLoad ? GetListLoader(type) : GetListSaver(type)); // Call the relevant list converter method
            }
            else
            {
                throw new InvalidOperationException($"Type '{type.FullName}' is not supported by {nameof(AutoConfiguration)}.");
            }
        }

        /// <summary>Types that can be duplicated by just returning the same instance.</summary>
        public static HashSet<Type> StandardTypes = new()
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string)
        };

        /// <summary>Duplicates common object types properly automatically.</summary>
        public static object Duplicate(object origObj)
        {
            // This method is dirty, but only has to be called once per field per AutoConfig, during startup, so isn't *too* perf relevant.
            Type t = origObj.GetType();
            if (StandardTypes.Contains(t))
            {
                return origObj;
            }
            if (origObj is AutoConfiguration)
            {
                return null;
            }
            if (origObj is IDictionary dict)
            {
                IDictionary result = t.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as IDictionary;
                foreach (DictionaryEntry subPair in dict)
                {
                    result.Add(Duplicate(subPair.Key), Duplicate(subPair.Value));
                }
                return result;
            }
            else if (origObj is Array array)
            {
                Array newArr = Array.CreateInstance(t.GetElementType(), array.Length);
                for (int i = 0; i < newArr.Length; i++)
                {
                    newArr.SetValue(Duplicate(array.GetValue(i)), i);
                }
                return newArr;
            }
            else if (origObj is IList list)
            {
                IList result = t.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as IList;
                foreach (object subObj in list)
                {
                    result.Add(Duplicate(subObj));
                }
                return result;
            }
            else if (origObj is ICollection)
            {
                List<FDSData> gennedList = GetListSaver(t).Invoke(null, new object[] { origObj }) as List<FDSData>;
                return GetListLoader(t).Invoke(null, new object[] { gennedList });
            }
            else
            {
                throw new InvalidOperationException($"Cannot duplicate object of type {t.FullName} - type not supported for AutoConfiguration");
            }
        }

        /// <summary>Generates a getter for <see cref="AutoConfiguration.Internal.SingleFieldData.GetValue"/>.</summary>
        public static Func<AutoConfiguration, object> GenerateValueGetter(FieldInfo field)
        {
            DynamicMethod genMethod = new($"Config{field.Name}ValueGetter", typeof(object), AutoConfigArray, typeof(AutoConfiguration).Module, true);
            ILGenerator targetILGen = genMethod.GetILGenerator();
            targetILGen.Emit(OpCodes.Ldarg_0); // Load the input parameter (stack=config)
            targetILGen.Emit(OpCodes.Ldfld, field); // Load the field (stack=field)
            if (field.FieldType.IsValueType)
            {
                targetILGen.Emit(OpCodes.Box, field.FieldType); // If necessary, box the value
            }
            targetILGen.Emit(OpCodes.Ret); // Return the value (stack clear)
            return genMethod.CreateDelegate<Func<AutoConfiguration, object>>();
        }

        /// <summary>Generates a setter for <see cref="AutoConfiguration.Internal.SingleFieldData.SetValue"/>.</summary>
        public static Action<AutoConfiguration, object> GenerateValueSetter(FieldInfo field)
        {
            DynamicMethod genMethod = new($"Config{field.Name}ValueSetter", typeof(void), new Type[] { typeof(AutoConfiguration), typeof(object) }, typeof(AutoConfiguration).Module, true);
            ILGenerator targetILGen = genMethod.GetILGenerator();
            targetILGen.Emit(OpCodes.Ldarg_0); // Load the config input parameter (stack=config)
            targetILGen.Emit(OpCodes.Ldarg_1); // Load the value input parameter (stack=config, value)
            if (field.FieldType.IsValueType)
            {
                targetILGen.Emit(OpCodes.Unbox_Any, field.FieldType);
            }
            targetILGen.Emit(OpCodes.Stfld, field); // Store value to the field (stack clear)
            targetILGen.Emit(OpCodes.Ret); // Return
            return genMethod.CreateDelegate<Action<AutoConfiguration, object>>();
        }

        /// <summary>Special helper bool tracker to avoid duplicate generation calls.</summary>
        public static bool AntiDuplicate;

        /// <summary>Generate the raw internal data for the specific config class type.</summary>
        /// <param name="type">The config class type.</param>
        /// <returns>The config data.</returns>
        public static AutoConfiguration.Internal.AutoConfigData GenerateData(Type type)
        {
            lock (GenerationLock)
            {
                if (TypeMap.TryGetValue(type, out AutoConfiguration.Internal.AutoConfigData result))
                {
                    return result;
                }
                if (AntiDuplicate)
                {
                    return null;
                }
                AutoConfiguration referenceDefaults;
                try
                {
                    AntiDuplicate = true;
                    referenceDefaults = type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as AutoConfiguration;
                }
                finally
                {
                    AntiDuplicate = false;
                }
                result = new AutoConfiguration.Internal.AutoConfigData();
                TypeMap[type] = result;
                // FDSSection Save(AutoConfiguration config) {
                DynamicMethod saveMethod = new("Save", typeof(FDSSection), SaveMethodInputTypeArray, typeof(AutoConfiguration).Module, true);
                ILGeneratorTracker saveILGen = new(saveMethod.GetILGenerator(), saveMethod, $"Save_{type.Name}");
                LocalBuilder saveOutputLocal = saveILGen.DeclareLocal(typeof(FDSSection)); // FDSSection output;
                saveILGen.Emit(OpCodes.Newobj, SectionConstructor); // output = new FDSSection();
                saveILGen.Emit(OpCodes.Stloc, saveOutputLocal);
                // void Load(AutoConfiguration config, FDSSection section) {
                DynamicMethod loadMethod = new("Load", typeof(void), LoadMethodInputTypeArray, typeof(AutoConfiguration).Module, true);
                ILGeneratorTracker loadILGen = new(loadMethod.GetILGenerator(), loadMethod, $"Load_{type.Name}");
                LocalBuilder loadgen_SectionLocal = loadILGen.DeclareLocal(typeof(FDSSection));
                LocalBuilder loadgen_DataLocal = loadILGen.DeclareLocal(typeof(FDSData));
                int fieldIndex = 0;
                static void genLoadMarkModified(ILGeneratorTracker loadILGen, AutoConfiguration.Internal.SingleFieldData fieldData)
                {
                    loadILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the config) (stack=config)
                    loadILGen.Emit(OpCodes.Ldfld, AutoConfigurationInternalDataField); // load the internal data field (stack=internal-data)
                    loadILGen.Emit(OpCodes.Ldfld, AutoConfigurationModifiedArrayField); // Load the modified array reference (stack=modified-array)
                    loadILGen.Emit(OpCodes.Ldc_I4, fieldData.Index); // Loads the field index integer (stack=modified-array,index)
                    loadILGen.Emit(OpCodes.Ldc_I4_1); // Loads a 'true' boolean (stack=modified-array,index,true)
                    loadILGen.Emit(OpCodes.Stelem_I1); // Store the true into the array (stack now clear)
                }
                foreach (FieldInfo field in type.GetRuntimeFields())
                {
                    if (field.IsStatic || field.FieldType == typeof(AutoConfiguration.LocalInternal))
                    {
                        continue;
                    }
                    AutoConfiguration.Internal.SingleFieldData fieldData = new()
                    {
                        Name = field.Name,
                        Index = fieldIndex++,
                        Field = field,
                        Default = Duplicate(field.GetValue(referenceDefaults)),
                        GetValue = GenerateValueGetter(field),
                        SetValue = GenerateValueSetter(field),
                        IsSection = typeof(AutoConfiguration).IsAssignableFrom(field.FieldType)
                    };
                    result.Fields.Add(field.Name.ToLowerFast(), fieldData);
                    saveILGen.Comment($"Begin save field {field.Name}");
                    loadILGen.Comment($"Begin load field {field.Name}");
                    saveILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the input config) (stack=input)
                    saveILGen.Emit(OpCodes.Ldfld, AutoConfigurationInternalDataField); // load the internal data field (stack=internal-data)
                    saveILGen.Emit(OpCodes.Ldfld, AutoConfigurationModifiedArrayField); // Load the modified array reference (stack=modified-array)
                    saveILGen.Emit(OpCodes.Ldc_I4, fieldData.Index); // Loads the field index integer (stack=modified-array,index)
                    saveILGen.Emit(OpCodes.Ldelem_I1); // Loads the boolean at the index (stack=modified-bool)
                    saveILGen.Emit(OpCodes.Ldarg_1); // load arg 1 (the boolean input 'save unmodified' input) (stack=modified-bool,save-unmodified-bool)
                    saveILGen.Emit(OpCodes.Or); // ORs the two bools - true output indicates save, false indicates skip (stack=actual bool)
                    Label skipLabel = saveILGen.DefineLabel();
                    saveILGen.Emit(OpCodes.Brfalse, skipLabel); // If false, skip past current part (stack clear)
                    // If true (do save), then:
                    saveILGen.Emit(OpCodes.Ldloc, saveOutputLocal); // load the output section (stack=output)
                    saveILGen.Emit(OpCodes.Ldstr, field.Name); // load the field name as a string (stack=output,name)
                    saveILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the input config) (stack=output,name,input)
                    saveILGen.Emit(OpCodes.Ldfld, field); // load the relevant field (stack=output,name,data)
                    if (typeof(AutoConfiguration).IsAssignableFrom(field.FieldType))
                    {
                        saveILGen.Emit(OpCodes.Ldarg_1); // load the save-unmodified parameter (stack=output,name,data,save-unmodified-bool)
                        saveILGen.Emit(OpCodes.Call, ConfigSaveMethod); // Call config.Save() (stack=output,name,out-data)
                        // /\ save | load \/
                        loadILGen.Emit(OpCodes.Ldarg_1); // load arg 1 (the FDS Section) (stack=section)
                        loadILGen.Emit(OpCodes.Ldstr, field.Name); // load the field name as a string (stack=section,name)
                        loadILGen.Emit(OpCodes.Call, SectionGetSectionMethod); // Call section.GetSection(name) (stack=sub-section)
                        loadILGen.Emit(OpCodes.Stloc, loadgen_SectionLocal); // Store sub-section to 'section' local (stack clear)
                        loadILGen.Emit(OpCodes.Ldloc, loadgen_SectionLocal); // Load the section local (stack=sub-section)
                        Label afterIfLabel = loadILGen.DefineLabel(); // if (value is null) {
                        loadILGen.Emit(OpCodes.Brfalse, afterIfLabel); // If the value is null, jump to after-the-if, otherwise continue ahead (stack clear)
                        loadILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the config) (stack=config)
                        loadILGen.Emit(OpCodes.Ldfld, field); // load the relevant field (stack=sub-config)
                        loadILGen.Emit(OpCodes.Ldloc, loadgen_SectionLocal); // Load the section local (stack=sub-config,sub-section)
                        loadILGen.Emit(OpCodes.Call, ConfigLoadMethod); // Call sub_config.Load(section) (stack now clear)
                        genLoadMarkModified(loadILGen, fieldData);
                        loadILGen.MarkLabel(afterIfLabel); // }
                    }
                    else
                    {
                        loadILGen.Emit(OpCodes.Ldarg_1); // load arg 1 (the FDS Section) (stack=section)
                        loadILGen.Emit(OpCodes.Ldstr, field.Name); // load the field name as a string (stack=section,name)
                        loadILGen.Emit(OpCodes.Call, SectionGetRootDataMethod); // call section.GetRootData(name) (stack=data)
                        loadILGen.Emit(OpCodes.Stloc, loadgen_DataLocal); // Store that data in the 'data' local (stack now clear)
                        loadILGen.Emit(OpCodes.Ldloc, loadgen_DataLocal); // Load the 'data' local (stack=data)
                        Label afterIfLabel = loadILGen.DefineLabel(); // if (value is not null) {
                        loadILGen.Emit(OpCodes.Brfalse, afterIfLabel); // If the value is null, jump to after-the-if, otherwise continue ahead (stack clear)
                        loadILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the config) (stack=config)
                        loadILGen.Emit(OpCodes.Ldloc, loadgen_DataLocal); // Load the 'data' local (stack=config,data)
                        EmitTypeConverter(field.FieldType, loadILGen, true); // call the type converter needed (stack=config,out-data)
                        loadILGen.Emit(OpCodes.Stfld, field); // Store to the relevant field on the config instance (stack was config,out-data - now clear)
                        genLoadMarkModified(loadILGen, fieldData);
                        loadILGen.MarkLabel(afterIfLabel); // }
                        // /\ load | save \/
                        EmitTypeConverter(field.FieldType, saveILGen, false); // call the type converter needed (stack=output,name,out-data-cleaned)
                    }
                    saveILGen.ValidateStackSizeIs("Save before FDSData init", 3);
                    AutoConfiguration.ConfigComment comment = field?.GetCustomAttribute<AutoConfiguration.ConfigComment>();
                    if (comment != null)
                    {
                        saveILGen.Emit(OpCodes.Ldstr, comment.Comments); // Load the comments onto stack (stack=output,name,out-data,commentstring)
                        saveILGen.Emit(OpCodes.Newobj, FDSDataObjectCommentConstructor); // new FDSData(out-data, commentstring) (stack=output,name,FDSData)
                    }
                    else
                    {
                        saveILGen.Emit(OpCodes.Newobj, FDSDataObjectConstructor); // new FDSData(out-data) (stack=output,name,FDSData)
                    }
                    saveILGen.Emit(OpCodes.Call, SectionSetRootDataMethod); // Call output.SetRootData(name, data); (stack was output,name,FDSData - now clear)
                    saveILGen.MarkLabel(skipLabel);
                }
                saveILGen.Emit(OpCodes.Ldloc, saveOutputLocal); // return output;
                saveILGen.Emit(OpCodes.Ret);
                loadILGen.Emit(OpCodes.Ret);
                result.SaveSection = saveMethod.CreateDelegate<Func<AutoConfiguration, bool, FDSSection>>();
                result.LoadSection = loadMethod.CreateDelegate<Action<AutoConfiguration, FDSSection>>();
                return result;
            }
        }
    }
}
