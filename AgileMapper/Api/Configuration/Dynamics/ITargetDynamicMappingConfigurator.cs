#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for configuring mappings from a <typeparamref name="TSource"/> to an ExpandoObject.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface ITargetDynamicMappingConfigurator<TSource> :
        IFullMappingConfigurator<TSource, IDictionary<string, object>>,
        ITargetDynamicConfigSettings<TSource>
    {
        /// <summary>
        /// Map the given <typeparamref name="TSource"/> member using a custom ExpandoObject member name.
        /// </summary>
        /// <typeparam name="TSourceMember">The source member's type.</typeparam>
        /// <param name="sourceMember">The source member to which to apply the configuration.</param>
        /// <returns>
        /// A CustomTargetDictionaryKeySpecifier with which to specify the custom key to use when mapping 
        /// the given <paramref name="sourceMember"/>.
        /// </returns>
        ICustomTargetDynamicMemberNameSpecifier<TSource> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMember);
    }
}
#endif