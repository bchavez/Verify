﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable UseObjectOrCollectionInitializer

namespace VerifyTests
{
    public delegate TMember ConvertMember<in TTarget, TMember>(TTarget target, TMember memberValue);

    public delegate object? ConvertMember(object? target, object? memberValue);

    public class SerializationSettings
    {
        static JsonConverter fileInfoConverter = new FileInfoConverter();
        static JsonConverter httpHeadersConverter = new HttpHeadersConverter();
        static JsonConverter directoryInfoConverter = new DirectoryInfoConverter();
        static JsonConverter stringEnumConverter = new StringEnumConverter();
        static JsonConverter delegateConverter = new DelegateConverter();
        static JsonConverter expressionConverter = new ExpressionConverter();
        static JsonConverter typeJsonConverter = new TypeJsonConverter();
        static JsonConverter methodInfoConverter = new MethodInfoConverter();
        static JsonConverter fieldInfoConverter = new FieldInfoConverter();
        static JsonConverter constructorInfoConverter = new ConstructorInfoConverter();
        static JsonConverter parameterInfoConverter = new ParameterInfoConverter();
        static JsonConverter versionConverter = new VersionConverter();
        static JsonConverter propertyInfoConverter = new PropertyInfoConverter();

        public SerializationSettings()
        {
            IgnoreMembersThatThrow<NotImplementedException>();
            IgnoreMembersThatThrow<NotSupportedException>();
            IgnoreMember<AggregateException>(x => x.InnerException);
            IgnoreMember<Exception>(x => x.Source);
            IgnoreMember<Exception>(x => x.HResult);
            MemberConverter<Exception, string>(x => x.StackTrace, (_, value) => Scrubbers.ScrubStackTrace(value));

            currentSettings = BuildSettings();
        }

        Dictionary<Type, List<string>> ignoredMembers = new();
        Dictionary<Type, Dictionary<string, ConvertMember>> membersConverters = new();
        List<string> ignoredByNameMembers = new();
        Dictionary<Type, List<Func<object, bool>>> ignoredInstances = new();

        public SerializationSettings Clone()
        {
            return new()
            {
                membersConverters = membersConverters.Clone(),
                ignoredMembers = ignoredMembers.Clone(),
                ignoredByNameMembers = ignoredByNameMembers.Clone(),
                ignoreEmptyCollections = ignoreEmptyCollections,
                ExtraSettings = ExtraSettings.Clone(),
                ignoreFalse = ignoreFalse,
                ignoreMembersThatThrow = ignoreMembersThatThrow.Clone(),
                ignoreMembersWithType = ignoreMembersWithType.Clone(),
                ignoredInstances = ignoredInstances.Clone(),
                scrubDateTimes = scrubDateTimes,
                scrubGuids = scrubGuids,
                includeObsoletes = includeObsoletes,
            };
        }

        public void IgnoreMember<T>(Expression<Func<T, object?>> expression)
        {
            Guard.AgainstNull(expression, nameof(expression));
            var member = expression.FindMember();
            IgnoreMember(member.DeclaringType!, member.Name);
        }

        public void IgnoreMember<T>(string name)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            IgnoreMember(typeof(T), name);
        }

        public void IgnoreMember(Type declaringType, string name)
        {
            Guard.AgainstNull(declaringType, nameof(declaringType));
            Guard.AgainstNullOrEmpty(name, nameof(name));
            if (!ignoredMembers.TryGetValue(declaringType, out var list))
            {
                ignoredMembers[declaringType] = list = new();
            }

            list.Add(name);
        }

        public void MemberConverter<TTarget, TMember>(
            Expression<Func<TTarget, TMember?>> expression,
            ConvertMember<TTarget, TMember?> converter)
        {
            Guard.AgainstNull(expression, nameof(expression));
            Guard.AgainstNull(converter, nameof(converter));
            var member = expression.FindMember();
            MemberConverter(
                member.DeclaringType!,
                member.Name,
                (target, memberValue) => converter((TTarget) target!, (TMember) memberValue!));
        }

        public void MemberConverter(Type declaringType, string name, ConvertMember converter)
        {
            Guard.AgainstNull(declaringType, nameof(declaringType));
            Guard.AgainstNull(converter, nameof(converter));
            Guard.AgainstNullOrEmpty(name, nameof(name));
            if (!membersConverters.TryGetValue(declaringType, out var list))
            {
                membersConverters[declaringType] = list = new();
            }

            list[name] = converter;
        }

        public void IgnoreMember(string name)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            ignoredByNameMembers.Add(name);
        }

        public void IgnoreMembers(params string[] names)
        {
            Guard.AgainstNullOrEmpty(names, nameof(names));
            foreach (var name in names)
            {
                IgnoreMember(name);
            }
        }

        public void IgnoreInstance<T>(Func<T, bool> shouldIgnore)
        {
            Guard.AgainstNull(shouldIgnore, nameof(shouldIgnore));
            var type = typeof(T);
            IgnoreInstance(
                type,
                target =>
                {
                    var arg = (T) target;
                    return shouldIgnore(arg);
                });
        }

        public void IgnoreInstance(Type type, Func<object, bool> shouldIgnore)
        {
            Guard.AgainstNull(type, nameof(type));
            Guard.AgainstNull(shouldIgnore, nameof(shouldIgnore));

            if (!ignoredInstances.TryGetValue(type, out var list))
            {
                ignoredInstances[type] = list = new();
            }

            list.Add(shouldIgnore);
        }

        List<Type> ignoreMembersWithType = new();

        public void IgnoreMembersWithType<T>()
        {
            ignoreMembersWithType.Add(typeof(T));
        }

        List<Func<Exception, bool>> ignoreMembersThatThrow = new();

        public void IgnoreMembersThatThrow<T>()
            where T : Exception
        {
            ignoreMembersThatThrow.Add(x => x is T);
        }

        public void IgnoreMembersThatThrow(Func<Exception, bool> item)
        {
            IgnoreMembersThatThrow<Exception>(item);
        }

        public void IgnoreMembersThatThrow<T>(Func<T, bool> item)
            where T : Exception
        {
            Guard.AgainstNull(item, nameof(item));
            ignoreMembersThatThrow.Add(
                x =>
                {
                    if (x is T exception)
                    {
                        return item(exception);
                    }

                    return false;
                });
        }

        bool ignoreEmptyCollections = true;

        public void DontIgnoreEmptyCollections()
        {
            ignoreEmptyCollections = false;
        }

        bool ignoreFalse = true;

        public void DontIgnoreFalse()
        {
            ignoreFalse = false;
        }

        bool scrubGuids = true;

        public void DontScrubGuids()
        {
            scrubGuids = false;
        }

        bool scrubDateTimes = true;

        public void DontScrubDateTimes()
        {
            scrubDateTimes = false;
        }

        public JsonSerializerSettings BuildSettings()
        {
            #region defaultSerialization

            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            #endregion

            settings.SerializationBinder = new ShortNameBinder();
            SharedScrubber scrubber = new(scrubGuids, scrubDateTimes, settings);
            settings.ContractResolver = new CustomContractResolver(
                ignoreEmptyCollections,
                ignoreFalse,
                includeObsoletes,
                ignoredMembers,
                ignoredByNameMembers,
                ignoreMembersWithType,
                ignoreMembersThatThrow,
                ignoredInstances,
                scrubber,
                membersConverters);
            var converters = settings.Converters;
            converters.Add(new StringConverter(scrubber));
            converters.Add(new GuidConverter(scrubber));
            converters.Add(new DateTimeConverter(scrubber));
            converters.Add(new DateTimeOffsetConverter(scrubber));
            converters.Add(fileInfoConverter);
            converters.Add(httpHeadersConverter);
            converters.Add(directoryInfoConverter);
            converters.Add(stringEnumConverter);
            converters.Add(expressionConverter);
            converters.Add(delegateConverter);
            converters.Add(versionConverter);
            converters.Add(typeJsonConverter);
            converters.Add(methodInfoConverter);
            converters.Add(fieldInfoConverter);
            converters.Add(constructorInfoConverter);
            converters.Add(propertyInfoConverter);
            converters.Add(parameterInfoConverter);
            converters.Add(new DictionaryConverter(ignoredByNameMembers));
            foreach (var extraSetting in ExtraSettings)
            {
                extraSetting(settings);
            }

            return settings;
        }

        public void AddExtraSettings(Action<JsonSerializerSettings> action)
        {
            Guard.AgainstNull(action, nameof(action));
            ExtraSettings.Add(action);
        }

        List<Action<JsonSerializerSettings>> ExtraSettings = new();
        internal JsonSerializerSettings currentSettings;

        internal void RegenSettings()
        {
            currentSettings = BuildSettings();
        }

        bool includeObsoletes;

        public void IncludeObsoletes()
        {
            includeObsoletes = true;
        }
    }
}
