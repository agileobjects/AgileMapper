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
    public class TargetMemberSelector
    {
        private readonly QualifiedMember _targetMember;
        private string _path;

        [DebuggerStepThrough]
        internal TargetMemberSelector(QualifiedMember targetMember)
        {
            _targetMember = targetMember;
        }

        /// <summary>
        /// Select target members by name. Constructor parameters will not be selected.
        /// </summary>
        public string Name => _targetMember.Name;

        /// <summary>
        /// Select target members by their nested member path. Constructor parameters will not be selected.
        /// </summary>
        public string Path => _path ?? (_path = GetPath());

        private string GetPath()
        {
            var path = _targetMember.GetPath();

            return path.StartsWith("Target.", StringComparison.Ordinal)
                ? path.Substring("Target.".Length)
                : path;
        }

        /// <summary>
        /// Select all target properties.
        /// </summary>
        public bool IsProperty => TargetMemberIs(MemberType.Property);

        /// <summary>
        /// Select any target properties which match the given <paramref name="propertyMatcher"/>.
        /// </summary>
        /// <param name="propertyMatcher">The predicate with which to select a matching property.</param>
        /// <returns>
        /// True if the target member is a property matching the given <paramref name="propertyMatcher"/>, 
        /// otherwise false.
        /// </returns>
        public bool IsPropertyMatching(Func<PropertyInfo, bool> propertyMatcher)
            => IsProperty && TargetMemberInfoMatches(propertyMatcher);

        /// <summary>
        /// Select all target fields.
        /// </summary>
        public bool IsField => TargetMemberIs(MemberType.Field);

        /// <summary>
        /// Select any target fields which match the given <paramref name="fieldMatcher"/>.
        /// </summary>
        /// <param name="fieldMatcher">The predicate with which to select a matching field.</param>
        /// <returns>
        /// True if the target member is a field matching the given <paramref name="fieldMatcher"/>, otherwise
        /// false.
        /// </returns>
        public bool IsFieldMatching(Func<FieldInfo, bool> fieldMatcher)
            => IsField && TargetMemberInfoMatches(fieldMatcher);

        /// <summary>
        /// Select all target set methods.
        /// </summary>
        public bool IsSetMethod => TargetMemberIs(MemberType.SetMethod);

        /// <summary>
        /// Select any target set methods which match the given <paramref name="setMethodMatcher"/>.
        /// </summary>
        /// <param name="setMethodMatcher">The predicate with which to select a matching set method.</param>
        /// <returns>
        /// True if the target member is a set method matching the given <paramref name="setMethodMatcher"/>, 
        /// otherwise false.
        /// </returns>
        public bool IsSetMethodMatching(Func<MethodInfo, bool> setMethodMatcher)
            => IsSetMethod && TargetMemberInfoMatches(setMethodMatcher);

        private bool TargetMemberIs(MemberType type)
            => _targetMember.LeafMember.MemberType == type;

        private bool TargetMemberInfoMatches<TMemberInfo>(Func<TMemberInfo, bool> matcher)
            where TMemberInfo : MemberInfo
        {
            return matcher.Invoke((TMemberInfo)_targetMember.LeafMember.MemberInfo);
        }

        /// <summary>
        /// Select target members with the given <typeparamref name="TMember">Type</typeparamref>. Constructor
        /// parameters will not be selected.
        /// </summary>
        /// <typeparam name="TMember">The Type of the target members to select.</typeparam>
        /// <returns>
        /// True if the target member has the given <typeparamref name="TMember">Type</typeparamref>, otherwise
        /// false.
        /// </returns>
        public bool HasType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                return _targetMember.Type == typeof(object);
            }

            return _targetMember.Type.IsAssignableTo(typeof(TMember));
        }

        /// <summary>
        /// Select target members with attributes of the given <typeparamref name="TAttribute">Type</typeparamref>.
        /// </summary>
        /// <typeparam name="TAttribute">The Type of attribute of the target members to select.</typeparam>
        /// <returns>
        /// True if the target member has an attribute of the given <typeparamref name="TAttribute">Type</typeparamref>, 
        /// otherwise false.
        /// </returns>
        public bool HasAttribute<TAttribute>()
            where TAttribute : Attribute
            => _targetMember.LeafMember.HasAttribute<TAttribute>();
    }
}