namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
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
            return otherPlanData.MappingData.SourceType == MappingData.SourceType &&
                   otherPlanData.MappingData.TargetType == MappingData.TargetType;
        }

        public override int GetHashCode() => 0;

        public MappingPlanData GetChildMappingPlanData(MethodCallExpression mapCall)
        {
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;

            var childMappingData = ObjectMappingDataFactory
                .ForChild(targetMemberName, dataSourceIndex, MappingData);

            return GetMappingPlanDataFor(childMappingData);
        }

        public MappingPlanData GetElementMappingPlanData(MethodCallExpression mapCall)
            => GetMappingPlanDataFor(ObjectMappingDataFactory.ForElement(MappingData));

        private MappingPlanData GetMappingPlanDataFor(IObjectMappingData mappingData)
        {
            var mappingLambda = mappingData.Mapper.MappingLambda;

            return new MappingPlanData(MappingContext, mappingLambda, mappingData);
        }
    }
}