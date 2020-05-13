//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticToolkit;
using FreneticUtilities.FreneticExtensions;

namespace FreneticUtilities.FreneticDataSyntax
{
    /// <summary>
    /// Extend this class to create an automatic FDS configuration utility.
    /// </summary>
    public abstract class AutoConfiguration
    {
        /// <summary>
        /// Adds comment lines to a configuration value.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public class ConfigComment : Attribute
        {
            /// <summary>
            /// The comments to add (separated via newline).
            /// </summary>
            public string Comments;

            /// <summary>
            /// Construct the config comment.
            /// </summary>
            /// <param name="_comment">The comment to add.</param>
            public ConfigComment(string _comment)
            {
                Comments = _comment;
            }
        }

        /// <summary>
        /// Internal tooling for <see cref="AutoConfiguration"/>.
        /// </summary>
        public static class Internal
        {
            /// <summary>
            /// Represents type-specific data for <see cref="AutoConfiguration"/>.
            /// </summary>
            public class AutoConfigData
            {
                /// <summary>
                /// Callable function that saves the config data to an FDS document.
                /// </summary>
                public Func<AutoConfiguration, FDSSection> SaveSection;

                /// <summary>
                /// Callable action that loads the config data from an FDS document.
                /// </summary>
                public Action<AutoConfiguration, FDSSection> LoadSection;
            }

            /// <summary>
            /// Helper class that represents the tools needed to convert <see cref="FDSData"/> to the final output type.
            /// </summary>
            public class DataConverter
            {
                /// <summary>
                /// Method that gets the data from an <see cref="FDSData"/> instance.
                /// </summary>
                public MethodInfo Getter;

                /// <summary>
                /// Method that gets the value of the data from a nullable instance (if needed).
                /// </summary>
                public MethodInfo ValueGrabber;
            }

            /// <summary>
            /// Static map of C# class type to the internal executable data needed to process it.
            /// </summary>
            public static ConcurrentDictionary<Type, AutoConfigData> TypeMap = new ConcurrentDictionary<Type, AutoConfigData>();

            /// <summary>
            /// Locker for generating new config data.
            /// </summary>
            public static LockObject GenerationLock = new LockObject();

            /// <summary>
            /// A reference to the <see cref="AutoConfiguration.Save"/> method.
            /// </summary>
            public static MethodInfo ConfigSaveMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Save));

            /// <summary>
            /// A reference to the <see cref="AutoConfiguration.Load"/> method.
            /// </summary>
            public static MethodInfo ConfigLoadMethod = typeof(AutoConfiguration).GetMethod(nameof(AutoConfiguration.Load));
            
            /// <summary>
            /// A reference to the <see cref="FDSSection.SetRootData(string, FDSData)"/> method.
            /// </summary>
            public static MethodInfo SectionSetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.SetRootData), new Type[] { typeof(string), typeof(FDSData) });

            /// <summary>
            /// A reference to the <see cref="FDSSection.GetSection(string)"/> method.
            /// </summary>
            public static MethodInfo SectionGetSectionMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetSection), new Type[] { typeof(string) });

            /// <summary>
            /// A reference to the <see cref="FDSSection.GetRootData(string)"/> method.
            /// </summary>
            public static MethodInfo SectionGetRootDataMethod = typeof(FDSSection).GetMethod(nameof(FDSSection.GetRootData), new Type[] { typeof(string) });

            /// <summary>
            /// A reference to the <see cref="FixNull{T}(T?)"/> method.
            /// </summary>
            public static MethodInfo FixNullMethod = typeof(Internal).GetMethod(nameof(FixNull));

            /// <summary>
            /// A reference to the <see cref="FDSSection"/> no-arguments constructor.
            /// </summary>
            public static ConstructorInfo SectionConstructor = typeof(FDSSection).GetConstructor(new Type[] { });

            /// <summary>
            /// A reference to the <see cref="FDSData"/> one-argument constructor.
            /// </summary>
            public static ConstructorInfo FDSDataObjectConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object) });

            /// <summary>
            /// A reference to the <see cref="FDSData"/> two-argument constructor.
            /// </summary>
            public static ConstructorInfo FDSDataObjectCommentConstructor = typeof(FDSData).GetConstructor(new Type[] { typeof(object), typeof(string) });

            /// <summary>
            /// A mapping of core object types to the method that converts <see cref="FDSData"/> to them.
            /// </summary>
            public static Dictionary<Type, DataConverter> FDSDataFieldsByType = new Dictionary<Type, DataConverter>(64);

            /// <summary>
            /// Init required static data.
            /// </summary>
            static Internal()
            {
                static void register(Type type, string propertyName)
                {
                    DataConverter converter = new DataConverter()
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
                register(typeof(double), nameof(FDSData.AsDouble));
                register(typeof(float), nameof(FDSData.AsFloat));
                register(typeof(string), nameof(FDSData.AsString));
                register(typeof(byte[]), nameof(FDSData.AsByteArray));
            }

            /// <summary>
            /// Utility method to perform "Nullable<typeparamref name="T"/>.Value" since CIL is very bad at emitting this properly.
            /// </summary>
            /// <typeparam name="T">The ValueType that will be Nullable.</typeparam>
            /// <param name="val">The instance to convert.</param>
            /// <returns>The non-nullable value.</returns>
            public static T FixNull<T>(T? val) where T : struct
            {
                return val.Value;
            }

            /// <summary>
            /// Emits the appropriate <see cref="FDSData"/> convert method for the applicable type.
            /// </summary>
            /// <param name="type">The type to load.</param>
            /// <param name="loadILGen">The IL Generator to emit to.</param>
            public static void EmitTypeLoader(Type type, ILGenerator loadILGen)
            {
                if (!FDSDataFieldsByType.TryGetValue(type, out DataConverter converter))
                {
                    throw new InvalidOperationException($"Type '{type.FullName}' is not supported by {nameof(AutoConfiguration)}.");
                }
                loadILGen.Emit(OpCodes.Callvirt, converter.Getter); // Call the loader method
                if (converter.ValueGrabber != null)
                {
                    loadILGen.Emit(OpCodes.Call, converter.ValueGrabber); // Call the nullable converter if needed
                }

                // TODO: List/Dictionary/etc. type support

            }

            /// <summary>
            /// Generate the raw internal data for the specific config class type.
            /// </summary>
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
                    result = new AutoConfigData();
                    TypeMap[type] = result;
                    // FDSSection Save(AutoConfiguration config) {
                    DynamicMethod saveMethod = new DynamicMethod("Save", typeof(FDSSection), new Type[] { typeof(AutoConfiguration) }, typeof(AutoConfiguration).Module, true);
                    ILGenerator saveILGen = saveMethod.GetILGenerator();
                    LocalBuilder saveOutputLocal = saveILGen.DeclareLocal(typeof(FDSSection)); // FDSSection output;
                    saveILGen.Emit(OpCodes.Newobj, SectionConstructor); // output = new FDSSection();
                    saveILGen.Emit(OpCodes.Stloc, saveOutputLocal);
                    // void Load(AutoConfiguration config, FDSSection section) {
                    DynamicMethod loadMethod = new DynamicMethod("Load", typeof(void), new Type[] { typeof(AutoConfiguration), typeof(FDSSection) }, typeof(AutoConfiguration).Module, true);
                    ILGenerator loadILGen = loadMethod.GetILGenerator();
                    foreach (FieldInfo field in type.GetRuntimeFields())
                    {
                        if (field.IsStatic || field.FieldType == typeof(AutoConfigData))
                        {
                            continue;
                        }
                        saveILGen.Emit(OpCodes.Ldloc, saveOutputLocal); // load the output section (stack=output)
                        saveILGen.Emit(OpCodes.Ldstr, field.Name); // load the field name as a string (stack=output,name)
                        saveILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the input config) (stack=output,name,input)
                        saveILGen.Emit(OpCodes.Ldfld, field); // load the relevant field (stack=output,name,data)
                        loadILGen.Emit(OpCodes.Ldarg_0); // load arg 0 (the config) (stack=config)
                        loadILGen.Emit(OpCodes.Ldarg_1); // load arg 1 (the FDS Section) (stack=config,section)
                        loadILGen.Emit(OpCodes.Ldstr, field.Name); // load the field name as a string (stack=config,section,name)
                        if (typeof(AutoConfiguration).IsAssignableFrom(field.FieldType))
                        {
                            saveILGen.Emit(OpCodes.Call, ConfigSaveMethod); // Call config.Save() (stack=output,name,out-data)
                            loadILGen.Emit(OpCodes.Call, SectionGetSectionMethod); // Call section.GetSection(name) (stack=config,sub-section)
                            loadILGen.Emit(OpCodes.Call, ConfigLoadMethod); // Call config.Load(section) (stack now clear)
                            loadILGen.Emit(OpCodes.Ldarg_0); // reload arg 0 (the config) (stack=config)
                        }
                        else
                        {
                            loadILGen.Emit(OpCodes.Call, SectionGetRootDataMethod); // call section.GetRootData(name) (stack=config,data)
                            EmitTypeLoader(field.FieldType, loadILGen); // call the type converter needed (stack=config,out-data)
                        }
                        if (field.FieldType.IsValueType)
                        {
                            saveILGen.Emit(OpCodes.Box, field.FieldType); // Box value types before sending along.
                        }
                        ConfigComment comment = field.GetCustomAttribute<ConfigComment>();
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
                        loadILGen.Emit(OpCodes.Stfld, field); // Store to the relevant field on the config instance (stack was config,out-data - now clear)
                    }
                    saveILGen.Emit(OpCodes.Ldloc, saveOutputLocal); // return output;
                    saveILGen.Emit(OpCodes.Ret);
                    loadILGen.Emit(OpCodes.Ret);
                    result.SaveSection = (Func<AutoConfiguration, FDSSection>)saveMethod.CreateDelegate(typeof(Func<AutoConfiguration, FDSSection>));
                    result.LoadSection = (Action<AutoConfiguration, FDSSection>)loadMethod.CreateDelegate(typeof(Action<AutoConfiguration, FDSSection>));
                    return result;
                }
            }
        }

        /// <summary>
        /// Internal-use only data.
        /// </summary>
        public Internal.AutoConfigData InternalData;

        /// <summary>
        /// Inits the <see cref="AutoConfiguration"/>.
        /// </summary>
        public AutoConfiguration()
        {
            Type type = GetType();
            if (!Internal.TypeMap.TryGetValue(type, out InternalData))
            {
                InternalData = Internal.GenerateData(type);
            }
        }

        /// <summary>
        /// Saves this <see cref="AutoConfiguration"/> to an <see cref="FDSSection"/>.
        /// </summary>
        /// <returns>The section object with all save data.</returns>
        public FDSSection Save()
        {
            return InternalData.SaveSection(this);
        }

        /// <summary>
        /// Loads this <see cref="AutoConfiguration"/> from an <see cref="FDSSection"/>.
        /// </summary>
        /// <param name="section">The section to load from.</param>
        public void Load(FDSSection section)
        {
            InternalData.LoadSection(this, section);
        }
    }
}
