namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Members;

    /// <summary>
    /// Provides a fluent interface to select source members by their characteristics.
    /// </summary>
    public class SourceMemberSelector : MemberSelectorBase
    {
        [DebuggerStepThrough]
        internal SourceMemberSelector(QualifiedMember sourceMember)
            : base(sourceMember)
        {
        }

        internal override string PathPrefix => "Source.";

        /// <summary>
        /// Select all set methods.
        /// </summary>
        public bool IsGetMethod => MemberIs(MemberType.GetMethod);

        /// <summary>
        /// Select any source get methods which match the given <paramref name="getMethodMatcher"/>.
        /// </summary>
        /// <param name="getMethodMatcher">The predicate with which to select a matching get method.</param>
        /// <returns>
        /// True if the source member is a get method matching the given <paramref name="getMethodMatcher"/>, 
        /// otherwise false.
        /// </returns>
        public bool IsGetMethodMatching(Func<MethodInfo, bool> getMethodMatcher)
            => IsGetMethod && MemberInfoMatches(getMethodMatcher);
    }
}