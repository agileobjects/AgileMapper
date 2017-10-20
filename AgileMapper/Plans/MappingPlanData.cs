namespace AgileObjects.AgileMapper.Plans
{
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        private readonly IObjectMappingData _mappingData;
        private readonly MappingTypes _mappingTypes;

        public MappingPlanData(IObjectMappingData mappingData)
        {
            Lambda = mappingData.Mapper.MappingLambda;
            _mappingData = mappingData;
            _mappingTypes = mappingData.MapperKey.MappingTypes;
        }

        public LambdaExpression Lambda { get; }

        public MappingPlanData GetChildMappingPlanData(MethodCallExpression mapCall)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;
            var parentMappingData = GetMappingDataFor((ParameterExpression)mapCall.Object);

            if (!parentMappingData.MapperData.Context.IsStandalone)
            {
                EnsureMapperCreation(parentMappingData.Mapper);
            }

            var childMappingData = ObjectMappingDataFactory.ForChild(
                targetMemberName,
                dataSourceIndex,
                parentMappingData);

            return new MappingPlanData(childMappingData);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void EnsureMapperCreation(IObjectMapper mapper)
        {
            // Bit ugly: force the lazy-load of the parent mapping data's 
            // Mapper to populate its MapperData with required information
        }

        public MappingPlanData GetElementMappingPlanData(MethodCallExpression mapCall)
        {
            var enumerableMappingData = GetMappingDataFor((ParameterExpression)mapCall.Object);
            var elementMappingData = ObjectMappingDataFactory.ForElement(enumerableMappingData);

            return new MappingPlanData(elementMappingData);
        }

        private IObjectMappingData GetMappingDataFor(ParameterExpression mapCallSubject)
            => GetMappingDataFor(mapCallSubject, _mappingData);

        private static IObjectMappingData GetMappingDataFor(ParameterExpression mapCallSubject, IObjectMappingData mappingData)
        {
            if (TryGetMatchingMappingData(mapCallSubject, mappingData, out var matchingMappingData))
            {
                return matchingMappingData;
            }

            return mappingData.MapperData
                .ChildMapperDatas
                .Select(childMapperData => childMapperData.TargetMemberIsEnumerableElement()
                    ? ObjectMappingDataFactory.ForElement(mappingData)
                    : ObjectMappingDataFactory.ForChild(
                        childMapperData.TargetMember.RegistrationName,
                        childMapperData.DataSourceIndex,
                        mappingData))
                .Select(childMappingData => GetMappingDataFor(mapCallSubject, childMappingData))
                .First(candidateMappingData => candidateMappingData != null);
        }

        private static bool TryGetMatchingMappingData(
            ParameterExpression mapCallSubject,
            IObjectMappingData mappingData,
            out IObjectMappingData matchingMappingData)
        {
            var mapperData = mappingData.MapperData;

            if ((mapCallSubject.Type == mapperData.MappingDataObject.Type) &&
                (mapCallSubject.Name == mapperData.MappingDataObject.Name))
            {
                matchingMappingData = mappingData;
                return true;
            }

            var subjectArgumentTypes = mapCallSubject.Type.GetGenericArguments();
            var subjectSourceType = subjectArgumentTypes.First();
            var subjectTargetType = subjectArgumentTypes.Last();

            if ((mapperData.SourceType == subjectSourceType) &&
                mapperData.TargetType.IsAssignableFrom(subjectTargetType))
            {
                matchingMappingData = mappingData;
                return true;
            }

            matchingMappingData = null;
            return false;
        }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            // ReSharper disable once PossibleNullReferenceException
            return otherPlanData._mappingTypes.Equals(_mappingTypes);
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;

        public string GetDescription() => MappingPlanFunction.For(Lambda, _mappingData.MapperData);
    }
}