namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for configuring mappings from a <typeparamref name="TSource"/> to a 
    /// Dictionary{string, <typeparamref name="TValue"/>}.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TValue">
    /// The type of values stored in the Dictionary to which the configurations will apply.
    /// </typeparam>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface ITargetDictionaryMappingConfigurator<TSource, TValue> :
        IFullMappingConfigurator<TSource, Dictionary<string, TValue>>,
        ITargetDictionaryConfigSettings<TSource, TValue>
    {
        /// <summary>
        /// Map the given <typeparamref name="TSource"/> member using a custom Dictionary key.
        /// </summary>
        /// <typeparam name="TSourceMember">The source member's type.</typeparam>
        /// <param name="sourceMember">The source member to which to apply the configuration.</param>
        /// <returns>
        /// A ICustomTargetDictionaryKeySpecifier with which to specify the custom key to use when mapping 
        /// the given <paramref name="sourceMember"/>.
        /// </returns>
        ICustomTargetDictionaryKeySpecifier<TSource, TValue> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMember);
    }
}