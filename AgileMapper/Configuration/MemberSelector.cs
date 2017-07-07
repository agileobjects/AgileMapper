namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;

    /// <summary>
    /// Provides a fluent interface to select members by their characteristics.
    /// </summary>
    public class TargetMemberSelector
    {
        private readonly List<Func<IBasicMapperData, bool>> _memberTests;

        [DebuggerStepThrough]
        internal TargetMemberSelector()
        {
            _memberTests = new List<Func<IBasicMapperData, bool>>();
        }

        /// <summary>
        /// Select target members with the given <typeparamref name="TMember">Type</typeparamref>.
        /// </summary>
        /// <typeparam name="TMember">The Type of the target members to select.</typeparam>
        /// <returns>The TargetMemberSelector, to allow addition of further selection criteria.</returns>
        public TargetMemberSelector HasType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                throw new MappingConfigurationException(
                    "Ignoring target members of type object would ignore everything!");
            }

            _memberTests.Add(md => typeof(TMember).IsAssignableFrom(md.TargetMember.Type));
            return this;
        }

        internal bool Matches(IBasicMapperData mapperData)
        {
            return _memberTests.All(test => test.Invoke(mapperData));
        }
    }
}