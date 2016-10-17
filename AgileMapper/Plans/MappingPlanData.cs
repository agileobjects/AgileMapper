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

        public MappingPlanData GetObjectMappingPlanData(MethodCallExpression mapCall)
        {
            var sourceType = mapCall.Arguments[0].Type;
            var targetType = mapCall.Arguments[1].Type;
            var targetMemberName = (string)((ConstantExpression)mapCall.Arguments[2]).Value;
            var dataSourceIndex = (int)((ConstantExpression)mapCall.Arguments[3]).Value;

            var childMappingData = ObjectMappingDataFactory.ForChildByTypes(
                sourceType,
                targetType,
                targetMemberName,
                dataSourceIndex,
                MappingData);

            var mappingLambda = childMappingData.CreateMapper().MappingLambda;

            return new MappingPlanData(MappingContext, mappingLambda, childMappingData);
        }

        public MappingPlanData GetElementMappingPlanData(MethodCallExpression mapCall)
        {
            var elementMappingData = ObjectMappingDataFactory.ForElementByTypes(
                mapCall.Arguments[0].Type,
                mapCall.Arguments[1].Type,
                0,
                MappingData);

            var mappingLambda = elementMappingData.CreateMapper().MappingLambda;

            return new MappingPlanData(MappingContext, mappingLambda, elementMappingData);
        }
    }
}