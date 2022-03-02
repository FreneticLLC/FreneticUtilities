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
    /// <summary>Extend this class to create an automatic FDS configuration utility.</summary>
    public abstract class AutoConfiguration
    {
        /// <summary>Adds comment lines to a configuration value.</summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ConfigComment : Attribute
        {
            /// <summary>The comments to add (separated via newline).</summary>
            public string Comments;

            /// <summary>Construct the config comment.</summary>
            /// <param name="_comment">The comment to add.</param>
            public ConfigComment(string _comment)
            {
                Comments = _comment;
            }
        }

        /// <summary>Internal tooling for <see cref="AutoConfiguration"/>.</summary>
        public static class Internal
        {
            /// <summary>Represents type-specific data for <see cref="AutoConfiguration"/>.</summary>
            public class AutoConfigData
            {
                /// <summary>Callable function that saves the config data to an FDS document.</summary>
                public Func<AutoConfiguration, bool, FDSSection> SaveSection;

                /// <summary>Callable action that loads the config data from an FDS document.</summary>
                public Action<AutoConfiguration, FDSSection> LoadSection;

                /// <summary>All fields for this instance, mapped from Name.ToLowerFast() to full field data.</summary>
                public Dictionary<string, SingleFieldData> Fields = new();
            }

            /// <summary>Container for important data about a single field in an <see cref="AutoConfiguration"/> class.</summary>
            public class SingleFieldData
            {
                /// <summary>The full name of the field.</summary>
                public string Name;

                /// <summary>The index of this field for special data arrays.</summary>
                public int Index;

                /// <summary>The raw C# field reference.</summary>
                public FieldInfo Field;

                /// <summary>Whether this is a sub-section.</summary>
                public bool IsSection;

                /// <summary>A function to get the current value of the field.</summary>
                public Func<AutoConfiguration, object> GetValue;

                /// <summary>A function to set the current value of the field.</summary>
                public Action<AutoConfiguration, object> SetValue;

                /// <summary>The default value of this field.</summary>
                public object Default;

                /// <summary>Action to fire when the field is changed by the standard 'Set' methods.</summary>
                public Action OnChanged;
            }

            /// <summary>Helper class that represents the tools needed to convert <see cref="FDSData"/> to the final output type.</summary>
            public class DataConverter
            {
                /// <summary>Method that gets the data from an <see cref="FDSData"/> instance.</summary>
                public MethodInfo Getter;

                /// <summary>Method that gets the value of the data from a nullable instance (if needed).</summary>
                public MethodInfo ValueGrabber;
            }

            /// <summary>Static map of C# class type to the internal executable data needed to process it.</summary>
            public static ConcurrentDictionary<Type, AutoConfigData> TypeMap = new();

            /// <summary>Locker for generating new config data.</summary>
            public static LockObject GenerationLock = new();

            /// <summary>A 1-value type array, with the value being <see cref="AutoConfiguration"/>.</summary>
            public static Type[] AutoConfigArray = new Type[] { typeof(AutoConfiguration) };

            /// <summary>Array of types for input to <see cref="AutoConfigData.SaveSection"/>.</summary>
            public static Type[] SaveMethodInputTypeArray = new Type[] { typeof(AutoConfiguration), typeof(bool) };

            /// <summary>Array of types for input to <see cref="AutoConfigData.LoadSection"/>.</summary>
            public static Type[] LoadMethodInputTypeArray = new Type[] { typeof(AutoConfiguration), typeof(FDSSection) };

            /// <summary>A reference to the <see cref="Save"/> method.</summary>
            public static MethodInfo ConfigSaveMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Save));

            /// <summary>A reference to the <see cref="Load"/> method.</summary>
            public static MethodInfo ConfigLoadMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Load));

            /// <summary>A reference to the <see cref="FDSSection.SetRootData(string, FDSData)"/> method.</summary>
            public static MethodInfo SectionSetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.SetRootData), new Type[] { typeof(string), typeof(FDSData) });

            /// <summary>A reference to the <see cref="FDSSection.GetSection(string)"/> method.</summary>
            public static MethodInfo SectionGetSectionMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetSection), new Type[] { typeof(string) });

            /// <summary>A reference to the <see cref="FDSSection.GetRootData(string)"/> method.</summary>
            public static MethodInfo SectionGetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetRootData), new Type[] { typeof(string) });

            /// <summary>A reference to the <see cref="FixNull{T}(T?)"/> method.</summary>
            public static MethodInfo FixNullMethod = typeof(Internal).GetMethod(nameof(FixNull));

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

            /// <summary>A reference to the <see cref="FDSSection"/> no-arguments constructor.</summary>
            public static ConstructorInfo SectionConstructor = typeof(FDSSection).GetConstructor(Array.Empty<Type>());

            /// <summary>A reference to the <see cref="FDSData"/> one-argument constructor.</summary>
            public static ConstructorInfo FDSDataObjectConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object) });

            /// <summary>A reference to the <see cref="FDSData"/> two-arguments constructor.</summary>
            public static ConstructorInfo FDSDataObjectCommentConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object), typeof(string) });

            /// <summary>A reference to the <see cref="List{FDSData}"/> of <see cref="FDSData"/> no-arguments constructor.</summary>
            public static ConstructorInfo FDSDataListConstructor = typeof(List<FDSData>).GetConstructor(Array.Empty<Type>());

            /// <summary>A reference to <see cref="InternalData"/>.</summary>
            public static FieldInfo AutoConfigurationInternalDataField = typeof(AutoConfiguration).GetField(nameof(InternalData));

            /// <summary>A reference to <see cref="LocalInternal.IsFieldModified"/>.</summary>
            public static FieldInfo AutoConfigurationModifiedArrayField = typeof(LocalInternal).GetField(nameof(LocalInternal.IsFieldModified));

            /// <summary>A mapping of core object types to the method that converts <see cref="FDSData"/> to them.</summary>
            public static Dictionary<Type, DataConverter> FDSDataFieldsByType = new(64);

            /// <summary>Init required static data.</summary>
            static Internal()
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
                else if (typeof(ICollection).IsAssignableFrom(type))
                {
                    if (doLoad)
                    {
                        targetILGen.Emit(OpCodes.Call, FDSDataAsDataListGetter);
                    }
                    targetILGen.Emit(OpCodes.Call, doLoad ? GetListLoader(type) : GetListSaver(type)); // Call the relevant list converter method
                }

                // TODO: Dictionary/...? type support

                else
                {
                    throw new InvalidOperationException($"Type '{type.FullName}' is not supported by {nameof(AutoConfiguration)}.");
                }
            }

            /// <summary>Types that can be duplicated by just returning the same instance.</summary>
            public static HashSet<Type> StandardTypes = new() {
                typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort),
                typeof(byte), typeof(sbyte), typeof(char),
                typeof(float), typeof(double), typeof(decimal),
                typeof(bool), typeof(string) };

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

            /// <summary>Generates a getter for <see cref="SingleFieldData.GetValue"/>.</summary>
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

            /// <summary>Generates a setter for <see cref="SingleFieldData.SetValue"/>.</summary>
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
            public static AutoConfigData GenerateData(Type type)
            {
                lock (GenerationLock)
                {
                    if (TypeMap.TryGetValue(type, out AutoConfigData result))
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
                    result = new AutoConfigData();
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
                    static void genLoadMarkModified(ILGeneratorTracker loadILGen, SingleFieldData fieldData)
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
                        if (field.IsStatic || field.FieldType == typeof(LocalInternal))
                        {
                            continue;
                        }
                        SingleFieldData fieldData = new()
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
                        ConfigComment comment = field?.GetCustomAttribute<ConfigComment>();
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

        /// <summary>Internal data local to this instance.</summary>
        public struct LocalInternal
        {
            /// <summary>Shared static data for this config type.</summary>
            public Internal.AutoConfigData SharedData;

            /// <summary>Array of field indices to a boolean indicating whether that field has been modified.</summary>
            public bool[] IsFieldModified;
        }

        /// <summary>Internal-use only data.</summary>
        public LocalInternal InternalData;

        /// <summary>Inits the <see cref="AutoConfiguration"/>.</summary>
        public AutoConfiguration()
        {
            Type type = GetType();
            if (!Internal.TypeMap.TryGetValue(type, out InternalData.SharedData))
            {
                InternalData.SharedData = Internal.GenerateData(type);
            }
            if (InternalData.SharedData is null)
            {
                return;
            }
            InternalData.IsFieldModified = new bool[InternalData.SharedData.Fields.Count];
        }

        /// <summary>Saves this <see cref="AutoConfiguration"/> to an <see cref="FDSSection"/>.</summary>
        /// <param name="includeUnmodified">If true, unmodified fields are stored. If false, unmodified fields are not stored.</param>
        /// <returns>The section object with all save data.</returns>
        public FDSSection Save(bool includeUnmodified)
        {
            return InternalData.SharedData.SaveSection(this, includeUnmodified);
        }

        /// <summary>Loads this <see cref="AutoConfiguration"/> from an <see cref="FDSSection"/>.</summary>
        /// <param name="section">The section to load from.</param>
        public void Load(FDSSection section)
        {
            InternalData.SharedData.LoadSection(this, section);
        }

        /// <summary>Tries to get the internal data for the given field by name.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para></summary>
        /// <returns>The internal data, or null if invalid.</returns>
        public Internal.SingleFieldData TryGetFieldInternalData(string field, out AutoConfiguration sectionAbove, bool markModified = false)
        {
            field = field.ToLowerFast();
            int dot = field.IndexOf('.');
            if (dot > 0)
            {
                string thisField = field[..dot];
                if (!InternalData.SharedData.Fields.TryGetValue(thisField, out Internal.SingleFieldData sectionData) || !sectionData.IsSection)
                {
                    sectionAbove = null;
                    return null;
                }
                string subPath = field[(dot + 1)..];
                if (sectionData.GetValue(this) is not AutoConfiguration subSection)
                {
                    sectionAbove = null;
                    return null;
                }
                if (markModified)
                {
                    InternalData.IsFieldModified[sectionData.Index] = true;
                }
                return subSection.TryGetFieldInternalData(subPath, out sectionAbove);
            }
            sectionAbove = this;
            if (!InternalData.SharedData.Fields.TryGetValue(field, out Internal.SingleFieldData data))
            {
                return null;
            }
            if (markModified)
            {
                InternalData.IsFieldModified[data.Index] = true;
            }
            return data;
        }

        /// <summary>Adds a changed event handler to the given field by name.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para></summary>
        /// <exception cref="ArgumentException">If the field name is invalid.</exception>
        public void RegisterChangedEventOrThrow(string field, Action changedEventHandler)
        {
            if (!TryRegisterChangedEvent(field, changedEventHandler))
            {
                throw new ArgumentException($"Invalid field path '{field}'");
            }
        }

        /// <summary>Tries to add a changed event handler to the given field by name.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para>
        /// <para>Exceptions can be thrown if object types are incorrect or null.</para></summary>
        /// <returns>True if set, false if not.</returns>
        public bool TryRegisterChangedEvent(string field, Action changedEventHandler)
        {
            Internal.SingleFieldData data = TryGetFieldInternalData(field, out _, true);
            if (data is null)
            {
                return false;
            }
            data.OnChanged += changedEventHandler;
            return true;
        }

        /// <summary>Tries to set the given field by name to the given value.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para>
        /// <para>Exceptions can be thrown if object types are incorrect or null.</para></summary>
        /// <returns>True if set, false if not.</returns>
        public bool TrySetFieldValue(string field, object value)
        {
            Internal.SingleFieldData data = TryGetFieldInternalData(field, out AutoConfiguration section, true);
            if (data is null)
            {
                return false;
            }
            data.SetValue(section, value);
            data.OnChanged?.Invoke();
            return true;
        }

        /// <summary>Gets the current value of the field by name, or the default if no value exists for the given path.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para>
        /// <para>Exceptions can be thrown if object types are incorrect or null.</para></summary>
        public T GetFieldValueOrDefault<T>(string field, T def = default)
        {
            Internal.SingleFieldData data = TryGetFieldInternalData(field, out AutoConfiguration section);
            if (data is null)
            {
                return def;
            }
            object result = data.GetValue(section);
            if (result is null)
            {
                return def;
            }
            if (!typeof(T).IsAssignableFrom(result.GetType()))
            {
                return def;
            }
            return (T)result;
        }

        /// <summary>Returns true if the named field is modified. Returns false if the field is unmodified, or paths are incorrect.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para></summary>
        public bool IsFieldModified(string field)
        {
            Internal.SingleFieldData data = TryGetFieldInternalData(field, out AutoConfiguration section);
            if (data is null)
            {
                return false;
            }
            return section.InternalData.IsFieldModified[data.Index];
        }

        /// <summary>Sets whether the named field is modified.
        /// <para>Case insensitive field names, allows dot-separated sub-paths.</para>
        /// <para>Exceptions can be thrown if object types are incorrect or null.</para></summary>
        /// <returns>True if applied, false if failed.</returns>
        public bool TrySetFieldModified(string field, bool modified)
        {
            Internal.SingleFieldData data = TryGetFieldInternalData(field, out AutoConfiguration section);
            if (data is null)
            {
                return false;
            }
            // TODO: if modified is false, apply default value to the field?
            section.InternalData.IsFieldModified[data.Index] = modified;
            if (data.IsSection && !modified)
            {
                static void clearModified(AutoConfiguration config)
                {
                    foreach (Internal.SingleFieldData subData in config.InternalData.SharedData.Fields.Values)
                    {
                        config.InternalData.IsFieldModified[subData.Index] = false;
                        if (subData.IsSection)
                        {
                            clearModified(subData.GetValue(config) as AutoConfiguration);
                        }
                    }
                }
                clearModified(data.GetValue(section) as AutoConfiguration);
            }
            return true;
        }
    }
}
