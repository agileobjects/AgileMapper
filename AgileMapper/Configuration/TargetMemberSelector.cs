namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Members;

    /// <summary>
    /// Provides a fluent interface to select target members by their characteristics.
    /// </summary>
    public class TargetMemberSelector : MemberSelectorBase
    {
        [DebuggerStepThrough]
        internal TargetMemberSelector(QualifiedMember targetMember)
            : base(targetMember)
        {
        }

        internal override string PathPrefix => Member.RootTargetMemberName + ".";

        /// <summary>
        /// Select all set methods.
        /// </summary>
        public bool IsSetMethod => MemberIs(MemberType.SetMethod);

        /// <summary>
        /// Select any target set methods which match the given <paramref name="setMethodMatcher"/>.
        /// </summary>
        /// <param name="setMethodMatcher">The predicate with which to select a matching set method.</param>
        /// <returns>
        /// True if the target member is a set method matching the given <paramref name="setMethodMatcher"/>, 
        /// otherwise false.
        /// </returns>
        public bool IsSetMethodMatching(Func<MethodInfo, bool> setMethodMatcher)
            => IsSetMethod && MemberInfoMatches(setMethodMatcher);
    }
}