namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        public MappingPlanData(
            MappingContext mappingContext,
            LambdaExpression lambda,
            ObjectMapperData mapperData)
        {
            MappingContext = mappingContext;
            Lambda = lambda;
            MapperData = mapperData;
        }

        public MappingContext MappingContext { get; }

        public LambdaExpression Lambda { get; }

        public ObjectMapperData MapperData { get; }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            return otherPlanData.MapperData.SourceType == MapperData.SourceType &&
                   otherPlanData.MapperData.TargetType == MapperData.TargetType;
        }

        public override int GetHashCode() => 0;
    }
}