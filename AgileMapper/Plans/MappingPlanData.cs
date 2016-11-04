namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        public MappingPlanData(
            IMappingContext mappingContext,
            LambdaExpression lambda,
            IObjectMappingData mappingData)
        {
            MappingContext = mappingContext;
            Lambda = lambda;
            MappingData = mappingData;
        }

        public IMappingContext MappingContext { get; }

        public LambdaExpression Lambda { get; }

        public IObjectMappingData MappingData { get; }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            // ReSharper disable once PossibleNullReferenceException
            return otherPlanData.MappingData.MapperKey.MappingTypes.Equals(MappingData.MapperKey.MappingTypes);
        }

        public override int GetHashCode() => 0;

        public MappingPlanData GetChildMappingPlanData(MethodCallExpression mapCall)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;
            var parentMappingData = GetMappingDataFor((ParameterExpression)mapCall.Object, MappingData);

            if (!parentMappingData.MapperData.IsForStandaloneMapping)
            {
                EnsureMapperCreation(parentMappingData.Mapper);
            }

            var childMappingData = ObjectMappingDataFactory.ForChild(
                targetMemberName,
                dataSourceIndex,
                parentMappingData);

            return GetMappingPlanDataFor(childMappingData);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void EnsureMapperCreation(IObjectMapper mapper)
        {
            // Bit ugly: force the lazy-load the parent mapping data's Mapper 
            // to populate its MapperData with required information
        }

        public MappingPlanData GetElementMappingPlanData(MethodCallExpression mapCall)
        {
            var enumerableMappingData = GetMappingDataFor((ParameterExpression)mapCall.Object, MappingData);
            var elementMappingData = ObjectMappingDataFactory.ForElement(enumerableMappingData);

            return GetMappingPlanDataFor(elementMappingData);
        }

        private static IObjectMappingData GetMappingDataFor(ParameterExpression mapCallSubject, IObjectMappingData mappingData)
        {
            if (MappingDataObjectMatches(mapCallSubject, mappingData))
            {
                return mappingData;
            }

            foreach (var childMapperData in mappingData.MapperData.ChildMapperDatas)
            {
                var childMappingData = childMapperData.TargetMemberIsEnumerableElement()
                    ? ObjectMappingDataFactory.ForElement(mappingData)
                    : ObjectMappingDataFactory.ForChild(
                        childMapperData.TargetMember.RegistrationName,
                        childMapperData.DataSourceIndex,
                        mappingData);

                var matchingMappingData = GetMappingDataFor(mapCallSubject, childMappingData);

                if (matchingMappingData != null)
                {
                    return matchingMappingData;
                }
            }

            return null;
        }

        private static bool MappingDataObjectMatches(ParameterExpression mapCallSubject, IObjectMappingData mappingData)
        {
            return (mapCallSubject.Type == mappingData.MapperData.MappingDataObject.Type) &&
                   (mapCallSubject.Name == mappingData.MapperData.MappingDataObject.Name);
        }

        private MappingPlanData GetMappingPlanDataFor(IObjectMappingData mappingData)
        {
            var mappingLambda = mappingData.Mapper.MappingLambda;

            return new MappingPlanData(MappingContext, mappingLambda, mappingData);
        }
    }
}