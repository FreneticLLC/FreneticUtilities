//
// This file is part of Frenetic Utilities, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticUtilities source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticUtilities.FreneticDataSyntax;

/// <summary>Extend this class to create an automatic FDS configuration utility.</summary>
public abstract class AutoConfiguration
{
    /// <summary>Adds comment lines to a configuration value.</summary>
    /// <param name="_comment">The comment to add.</param>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigComment(string _comment) : Attribute
    {
        /// <summary>The comments to add (separated via newline).</summary>
        public string Comments = _comment;
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
            public Dictionary<string, SingleFieldData> Fields = [];
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
        if (!AutoConfigurationCodeGenerator.TypeMap.TryGetValue(type, out InternalData.SharedData))
        {
            InternalData.SharedData = AutoConfigurationCodeGenerator.GenerateData(type);
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
            return subSection.TryGetFieldInternalData(subPath, out sectionAbove, markModified);
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
