namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceAccessFinder : ExpressionVisitor
    {
        private readonly Expression _mappingDataObject;
        private int _numberOfAccesses;

        private SourceAccessFinder(Expression mappingDataObject)
        {
            _mappingDataObject = mappingDataObject;
        }

        public static bool MultipleAccessesExist(IMemberMapperData mapperData, Expression mappingExpression)
        {
            var finder = new SourceAccessFinder(mapperData.MappingDataObject);

            finder.Visit(mappingExpression);

            return finder._numberOfAccesses >= 5;
        }

        protected override Expression VisitMember(MemberExpression memberAccess)
        {
            if ((memberAccess.Expression == _mappingDataObject) &&
                (memberAccess.Member.Name == Member.RootSourceMemberName))
            {
                ++_numberOfAccesses;
            }

            return base.VisitMember(memberAccess);
        }
    }
}