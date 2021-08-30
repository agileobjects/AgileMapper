namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using DataSources;
    using MapperKeys;
    using Members;

    internal interface IObjectMappingData : IObjectMappingDataUntyped, IDataSourceSetInfo
    {
        bool IsRoot { get; }

        new IObjectMappingData Parent { get; }

        bool IsPartOfRepeatedMapping { get; set; }

        bool IsPartOfDerivedTypeMapping { get; }

        IObjectMappingData DeclaredTypeMappingData { get; }

        MappingTypes MappingTypes { get; }

        ObjectMapperKeyBase MapperKey { get; set; }

        bool MapperDataPopulated { get; }

        new ObjectMapperData MapperData { get; }

        IObjectMapper GetOrCreateMapper();

        IChildMemberMappingData GetChildMappingData(IMemberMapperData childMapperData);

        Type GetSourceMemberRuntimeType(IQualifiedMember childSourceMember);

        object MapStart();

        IObjectMappingData WithToTargetSource(IQualifiedMember sourceMember);

        IObjectMappingData WithDerivedTypes(Type sourceType, Type targetType);
    }

    /// <summary>
    /// Provides the data being used and services available at a particular point during a mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of source object being mapped from in the current context.</typeparam>
    /// <typeparam name="TTarget">The type of target object being mapped to in the current context.</typeparam>
    public interface IObjectMappingData<out TSource, TTarget> : IObjectMappingDataUntyped, IMappingData<TSource, TTarget>
    {
        /// <summary>
        /// Gets the data of the mapping context directly 'above' that described by the 
        /// <see cref="IObjectMappingData{TSource, TTarget}"/>.
        /// </summary>
        new IObjectMappingDataUntyped Parent { get; }

        /// <summary>
        /// Gets or sets the target object for the mapping context described by the 
        /// <see cref="IObjectMappingData{TSource, TTarget}"/>.
        /// </summary>
        new TTarget Target { get; set; }

        /// <summary>
        /// Gets or sets the object created by the current mapping context, if applicable.
        /// </summary>
        TTarget CreatedObject { get; set; }

        /// <summary>
        /// Map the given <paramref name="sourceElement"/> to the given <paramref name="targetElement"/>.
        /// </summary>
        /// <typeparam name="TSourceElement">
        /// The declared type of the given <paramref name="sourceElement"/>.
        /// </typeparam>
        /// <typeparam name="TTargetElement">
        /// The declared type of the given <paramref name="targetElement"/>.
        /// </typeparam>
        /// <param name="sourceElement">The source object from which to map.</param>
        /// <param name="targetElement">The target object to which to map.</param>
        /// <param name="elementIndex">
        /// The index of the current enumerable <paramref name="sourceElement"/> being mapped in the
        /// mapping context described by this <see cref="IObjectMappingData{TSource, TTarget}"/>, if
        /// applicable.
        /// </param>
        /// <param name="elementKey">
        /// The key of the current Dictionary KeyValuePair being mapped in the mapping context described
        /// by this <see cref="IObjectMappingData{TSource, TTarget}"/>, if applicable.
        /// </param>
        /// <returns>The element mapping result.</returns>
        TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey);

        /// <summary>
        /// Gets the <see cref="IObjectMappingData{TSource, TTarget}"/> as an 
        /// <see cref="IObjectMappingData{TNewSource, TTarget}"/> using the given
        /// <paramref name="newSource"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of the new source object to use.</typeparam>
        /// <param name="newSource">The new source object to use.</param>
        /// <returns>
        /// The <see cref="IObjectMappingData{TSource, TTarget}"/> as a 
        /// <see cref="IObjectMappingData{TNewSource, TTarget}"/>.
        /// </returns>
        IObjectMappingData<TNewSource, TTarget> WithSource<TNewSource>(TNewSource newSource);

        /// <summary>
        /// Gets the <see cref="IObjectMappingData{TSource, TTarget}"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> when the target object definitely
        /// cannot be converted to the given <typeparamref name="TNewTarget"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TNewTarget">The type of target object being mapped in the current context.</typeparam>
        /// <param name="isForDerivedTypeMapping">
        /// Whether the new, typed <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> is needed for the creation
        /// of a derived type mapping.
        /// </param>
        /// <returns>
        /// The <see cref="IObjectMappingData{TSource, TTarget}"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </returns>
        IObjectMappingData<TNewSource, TNewTarget> WithSourceType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class;

        /// <summary>
        /// Gets the <see cref="IObjectMappingData{TSource, TTarget}"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> when the source object definitely
        /// cannot be converted to the given <typeparamref name="TNewSource"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TNewTarget">The type of target object being mapped in the current context.</typeparam>
        /// <param name="isForDerivedTypeMapping">
        /// Whether the new, typed <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> is needed for the creation
        /// of a derived type mapping.
        /// </param>
        /// <returns>
        /// The <see cref="IObjectMappingData{TSource, TTarget}"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </returns>
        IObjectMappingData<TNewSource, TNewTarget> WithTargetType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewTarget : class;
    }
}