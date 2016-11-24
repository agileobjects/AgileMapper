namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

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
            if (MappingDataObjectMatches(mapCallSubject, mappingData))
            {
                return mappingData;
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
                .First(matchingMappingData => matchingMappingData != null);
        }

        private static bool MappingDataObjectMatches(ParameterExpression mapCallSubject, IObjectMappingData mappingData)
        {
            return (mapCallSubject.Type == mappingData.MapperData.MappingDataObject.Type) &&
                   (mapCallSubject.Name == mappingData.MapperData.MappingDataObject.Name);
        }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            // ReSharper disable once PossibleNullReferenceException
            return otherPlanData._mappingTypes.Equals(_mappingTypes);
        }

        public override int GetHashCode() => 0;

        public string GetDescription()
        {
            var sourceType = _mappingTypes.SourceType.GetFriendlyName();
            var targetType = _mappingTypes.TargetType.GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {_mappingData.MappingContext.RuleSet.Name}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{Lambda.ToReadableString()}".TrimStart();
        }
    }
}