namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Diagnostics;
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

        [DebuggerStepThrough]
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
        public string Path => _path ?? (_path = GetPath());

        private string GetPath()
        {
            var path = _member.GetPath();

            return path.StartsWith("Target.", StringComparison.Ordinal)
                ? path.Substring("Target.".Length)
                : path;
        }

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
        /// Select members with the given <typeparamref name="TMember">Type</typeparamref>. Constructor
        /// parameters will not be selected.
        /// </summary>
        /// <typeparam name="TMember">The Type of the members to select.</typeparam>
        /// <returns>
        /// True if the member has the given <typeparamref name="TMember">Type</typeparamref>, otherwise
        /// false.
        /// </returns>
        public bool HasType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                return _member.Type == typeof(object);
            }

            return _member.Type.IsAssignableTo(typeof(TMember));
        }

        /// <summary>
        /// Select members with attributes of the given <typeparamref name="TAttribute">Type</typeparamref>.
        /// </summary>
        /// <typeparam name="TAttribute">The Type of attribute of the members to select.</typeparam>
        /// <returns>
        /// True if the member has an attribute of the given <typeparamref name="TAttribute">Type</typeparamref>, 
        /// otherwise false.
        /// </returns>
        public bool HasAttribute<TAttribute>()
            where TAttribute : Attribute
            => _member.LeafMember.HasAttribute<TAttribute>();
    }
}