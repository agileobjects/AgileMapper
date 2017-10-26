namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Reflection;

    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingInlineConfigurator<TSource, TTarget> : IFullMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Configure how this mapper performs a mapping, inline.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }

        /// <summary>
        /// Scan the given <paramref name="assemblies"/> when looking for types derived from any source or 
        /// target type being mapped.
        /// </summary>
        /// <param name="assemblies">The assemblies in which to look for derived types.</param>
        /// <returns>
        /// This <see cref="IFullMappingInlineConfigurator{TSource, TTarget}"/> with which to configure further 
        /// settings for the source and target types being mapped.
        /// </returns>
        IFullMappingInlineConfigurator<TSource, TTarget> LookForDerivedTypesIn(params Assembly[] assemblies);

        /// <summary>
        /// Configure this mapper to pair the given <paramref name="enumMember"/> with a member of another enum Type.
        /// </summary>
        /// <typeparam name="TFirstEnum">The type of the first enum being paired.</typeparam>
        /// <param name="enumMember">The first enum member in the pair.</param>
        /// <returns>
        /// An EnumPairSpecifier with which to specify the enum member to which the given <paramref name="enumMember"/> 
        /// should be paired.
        /// </returns>
        EnumPairSpecifier<TSource, TTarget, TFirstEnum> PairEnum<TFirstEnum>(TFirstEnum enumMember) where TFirstEnum : struct;
    }
}