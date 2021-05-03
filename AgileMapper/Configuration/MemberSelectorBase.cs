namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides a fluent interface to select members by their characteristics.
    /// </summary>
    public abstract class MemberSelectorBase
    {
        private readonly QualifiedMember _member;
        private string _path;

        internal MemberSelectorBase(QualifiedMember member)
        {
            _member = member;
        }

        /// <summary>
        /// Select members by name. Constructor parameters will not be selected.
        /// </summary>
        public string Name => _member.Name;

        /// <summary>
        /// Select members by their nested path. Constructor parameters will not be selected.
        /// </summary>
        public string Path => _path ??= GetPath();

        private string GetPath()
        {
            var path = _member.GetPath();

            return path.StartsWith(PathPrefix, StringComparison.Ordinal)
                ? path.Substring(PathPrefix.Length)
                : path;
        }

        internal abstract string PathPrefix { get; }

        /// <summary>
        /// Select all properties.
        /// </summary>
        public bool IsProperty => MemberIs(MemberType.Property);

        /// <summary>
        /// Select any properties which match the given <paramref name="propertyMatcher"/>.
        /// </summary>
        /// <param name="propertyMatcher">The predicate with which to select a matching property.</param>
        /// <returns>
        /// True if the member is a property matching the given <paramref name="propertyMatcher"/>, 
        /// otherwise false.
        /// </returns>
        public bool IsPropertyMatching(Func<PropertyInfo, bool> propertyMatcher)
            => IsProperty && MemberInfoMatches(propertyMatcher);

        /// <summary>
        /// Select all fields.
        /// </summary>
        public bool IsField => MemberIs(MemberType.Field);

        /// <summary>
        /// Select any fields which match the given <paramref name="fieldMatcher"/>.
        /// </summary>
        /// <param name="fieldMatcher">The predicate with which to select a matching field.</param>
        /// <returns>
        /// True if the member is a field matching the given <paramref name="fieldMatcher"/>, otherwise
        /// false.
        /// </returns>
        public bool IsFieldMatching(Func<FieldInfo, bool> fieldMatcher)
            => IsField && MemberInfoMatches(fieldMatcher);

        internal bool MemberIs(MemberType type)
            => _member.LeafMember.MemberType == type;

        internal bool MemberInfoMatches<TMemberInfo>(Func<TMemberInfo, bool> matcher)
            where TMemberInfo : MemberInfo
        {
            return matcher.Invoke((TMemberInfo)_member.LeafMember.MemberInfo);
        }

        /// <summary>
        /// Select members with type <typeparamref name="TMember" />. Constructor parameters will not
        /// be selected if a target member ignore is configured.
        /// </summary>
        /// <typeparam name="TMember">The Type of the members to select.</typeparam>
        /// <returns>True if the member has type <typeparamref name="TMember"/>, otherwise false.</returns>
        public bool HasType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                return _member.Type == typeof(object);
            }

            return _member.Type.IsAssignableTo(typeof(TMember));
        }

        /// <summary>
        /// Select members with attributes of type <typeparamref name="TAttribute" />.
        /// </summary>
        /// <typeparam name="TAttribute">The Type of attribute of the members to select.</typeparam>
        /// <returns>
        /// True if the member has an attribute of type <typeparamref name="TAttribute" />, 
        /// otherwise false.
        /// </returns>
        public bool HasAttribute<TAttribute>()
            where TAttribute : Attribute
            => _member.LeafMember.HasAttribute<TAttribute>();
    }
}