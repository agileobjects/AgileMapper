namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MappingPlanData
    {
        public MappingPlanData(LambdaExpression lambda, IObjectMappingContext omc)
        {
            Lambda = lambda;
            Omc = omc;
        }

        public LambdaExpression Lambda { get; }

        public IObjectMappingContext Omc { get; }

        public override bool Equals(object obj)
        {
            var otherPlanData = (MappingPlanData)obj;

            return otherPlanData.Omc.SourceType == Omc.SourceType &&
                   otherPlanData.Omc.TargetType == Omc.TargetType;
        }

        public override int GetHashCode() => 0;
    }
}