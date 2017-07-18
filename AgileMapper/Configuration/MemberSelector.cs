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

        public bool IsField => _targetMember.LeafMember.MemberType == MemberType.Field;

        public bool IsSetMethod => _targetMember.LeafMember.MemberType == MemberType.SetMethod;

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