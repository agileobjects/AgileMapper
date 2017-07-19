namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Diagnostics;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;

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
        /// Select target members by name.
        /// </summary>
        public string Name => _targetMember.Name;

        /// <summary>
        /// Select target members by their nested member path.
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
        /// Select all target fields.
        /// </summary>
        public bool IsField => TargetMemberIs(MemberType.Field);

        /// <summary>
        /// Select all target set methods.
        /// </summary>
        public bool IsSetMethod => TargetMemberIs(MemberType.SetMethod);

        private bool TargetMemberIs(MemberType type)
            => _targetMember.LeafMember.MemberType == type;

        /// <summary>
        /// Select target members with the given <typeparamref name="TMember">Type</typeparamref>.
        /// </summary>
        /// <typeparam name="TMember">The Type of the target members to select.</typeparam>
        /// <returns>The TargetMemberSelector, to allow addition of further selection criteria.</returns>
        public bool HasType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                return _targetMember.Type == typeof(object);
            }

            return typeof(TMember).IsAssignableFrom(_targetMember.Type);
        }
    }
}