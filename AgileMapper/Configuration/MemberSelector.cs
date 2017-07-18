namespace AgileObjects.AgileMapper.Configuration
{
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
            return typeof(TMember).IsAssignableFrom(_targetMember.Type);
        }
    }
}