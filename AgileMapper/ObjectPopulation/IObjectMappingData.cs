namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using MapperKeys;
    using Members;

    internal interface IObjectMappingData : IObjectMappingDataUntyped
    {
        IMappingContext MappingContext { get; }

        bool IsRoot { get; }

        new IObjectMappingData Parent { get; }

        bool IsPartOfDerivedTypeMapping { get; }

        IObjectMappingData DeclaredTypeMappingData { get; }

        MappingTypes MappingTypes { get; }

        ObjectMapperKeyBase MapperKey { get; set; }

        ObjectMapperData MapperData { get; }

        IObjectMapper GetOrCreateMapper();

        IChildMemberMappingData GetChildMappingData(IMemberMapperData childMapperData);

        Type GetSourceMemberRuntimeType(IQualifiedMember childSourceMember);

        object MapStart();

        IObjectMappingData WithSource(IQualifiedMember newSourceMember);

        IObjectMappingData WithTypes(Type newSourceType, Type newTargetType, bool isForDerivedTypeMapping = true);
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
        /// Map the given <paramref name="sourceValue"/> to the given <paramref name="targetValue"/>.
        /// </summary>
        /// <typeparam name="TDeclaredSource">
        /// The declared type of the given <paramref name="sourceValue"/>.
        /// </typeparam>
        /// <typeparam name="TDeclaredTarget">
        /// The declared type of the given <paramref name="targetValue"/>.
        /// </typeparam>
        /// <param name="sourceValue">The source object from which to map.</param>
        /// <param name="targetValue">The target object to which to map.</param>
        /// <param name="targetMemberName">The name of the target member being mapped.</param>
        /// <param name="dataSourceIndex">
        /// The index of the data source being used to perform the mapping.
        /// </param>
        /// <returns>The mapping result.</returns>
        TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex);

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
        /// <param name="enumerableIndex">
        /// The index of the current enumerable the elements of which are being mapped.
        /// </param>
        /// <returns>The element mapping result.</returns>
        TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex);

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