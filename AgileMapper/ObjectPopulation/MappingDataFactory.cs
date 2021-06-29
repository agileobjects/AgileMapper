namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Enumerables;
    using Extensions.Internal;
    using MapperKeys;
    using NetStandardPolyfills;

    /// <summary>
    /// Factory class to create <see cref="IObjectMappingData{TSource,TTarget}"/> instances. This
    /// class is used internally by mapping functions.
    /// </summary>
    public static class MappingDataFactory
    {
        private static MethodInfo _forChildMethod;
        private static MethodInfo _forElementMethod;

        internal static MethodInfo ForChildMethod =>
            _forChildMethod ??= typeof(MappingDataFactory).GetPublicStaticMethod(nameof(ForChild));

        internal static MethodInfo ForElementMethod
            => _forElementMethod ??= typeof(MappingDataFactory).GetPublicStaticMethod(nameof(ForElement));

        /// <summary>
        /// Creates an <see cref="IObjectMappingData{TSource,TTarget}"/> instance for the given data,
        /// for the mapping of a runtime-typed child member.
        /// </summary>
        /// <typeparam name="TSource">The declared type of the source object.</typeparam>
        /// <typeparam name="TTarget">The declared type of the target object.</typeparam>
        /// <param name="source">The child source object.</param>
        /// <param name="target">The child target object.</param>
        /// <param name="elementIndex">The index of the current enumerable element being mapped, if applicable.</param>
        /// <param name="elementKey">The current Dictionary KeyValuePair being mapped, if applicable.</param>
        /// <param name="targetMemberRegistrationName">The name of the target member being mapped.</param>
        /// <param name="dataSourceIndex">The index of the source value being used.</param>
        /// <param name="parent">The <see cref="IObjectMappingDataUntyped"/> describing the parent mapping context.</param>
        /// <returns>An <see cref="IObjectMappingData{TSource,TTarget}"/> instance for the given data.</returns>
        public static IObjectMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                MappingTypes<TSource, TTarget>.Fixed,
                targetMemberRegistrationName,
                dataSourceIndex);

            var mappingData = CreateMappingData(source, target, elementIndex, elementKey, mapperKey, parent);

            if (!mappingData.SubMappingNeeded(out var parentMappingData))
            {
                return mappingData;
            }

            var mapperData = parentMappingData.MapperData.ChildMapperDatasOrEmpty.FirstOrDefault(md =>
                (md.DataSourceIndex == dataSourceIndex) &&
                (md.TargetMember.RegistrationName == targetMemberRegistrationName));

            if (mapperData != null)
            {
                mappingData.SetMapper(mapperData.Mapper);
            }

            return mappingData;
        }

        /// <summary>
        /// Creates an <see cref="IObjectMappingData{TSource,TTarget}"/> instance for the given data,
        /// for the mapping of a runtime-typed collection element.
        /// </summary>
        /// <typeparam name="TSourceElement">The declared type of the source element.</typeparam>
        /// <typeparam name="TTargetElement">The declared type of the target element.</typeparam>
        /// <param name="sourceElement">The source element object.</param>
        /// <param name="targetElement">The target element object.</param>
        /// <param name="elementIndex">The index of the current enumerable element being mapped, if applicable.</param>
        /// <param name="elementKey">The current Dictionary KeyValuePair being mapped, if applicable.</param>
        /// <param name="parent">The <see cref="IObjectMappingDataUntyped"/> describing the parent mapping context.</param>
        /// <returns>An <see cref="IObjectMappingData{TSource,TTarget}"/> instance for the given data.</returns>
        public static IObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes<TSourceElement, TTargetElement>.Fixed);

            var mappingData = CreateMappingData(sourceElement, targetElement, elementIndex, elementKey, mapperKey, parent);

            if (mappingData.SubMappingNeeded(out var parentMappingData))
            {
                mappingData.SetMapper(
                    parentMappingData.MapperData.ChildMapperDatas.First().Mapper);
            }

            return mappingData;
        }

        private static ObjectMappingData<TSource, TTarget> CreateMappingData<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            ObjectMapperKeyBase mapperKey,
            IObjectMappingDataUntyped parent)
        {
            var mappingDataParent = (IObjectMappingData)parent;

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                elementIndex,
                elementKey,
                mapperKey.MappingTypes,
                mappingDataParent.MappingContext,
                mappingDataParent)
            {
                MapperKey = mapperKey
            };
        }

        private static bool SubMappingNeeded(this IObjectMappingData mappingData, out IObjectMappingData parentMappingData)
        {
            parentMappingData = GetParentMappingData(mappingData);

            return parentMappingData.MapperDataPopulated &&
                   parentMappingData.MapperData.Context.NeedsRuntimeTypedMapping;
        }

        private static IObjectMappingData GetParentMappingData(IObjectMappingData mappingData)
        {
            return mappingData.Parent.IsPartOfDerivedTypeMapping
                ? mappingData.Parent.DeclaredTypeMappingData
                : mappingData.Parent;
        }
    }
}