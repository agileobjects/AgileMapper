namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using System.Collections.Generic;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class SourceMemberDataSource : DataSourceBase
    {
        private SourceMemberDataSource(
            IQualifiedMember sourceMember,
            Expression sourceMemberValue,
            IMemberMapperData mapperData)
            : base(
                  sourceMember,
                  mapperData.GetValueConversion(sourceMemberValue, mapperData.TargetMember.Type),
                  mapperData)
        {
            SourceMemberTypeTest = CreateSourceMemberTypeTest(sourceMemberValue, mapperData);
        }

        private static Expression CreateSourceMemberTypeTest(Expression value, IMemberMapperData mapperData)
        {
            var parent = value;
            var typeTests = new List<Expression>();

            while (parent != mapperData.SourceObject)
            {
                if (parent.NodeType == ExpressionType.Convert)
                {
                    var cast = (UnaryExpression)parent;
                    parent = cast.Operand;

                    typeTests.Insert(0, GetRuntimeTypeCheck(cast, mapperData));
                }

                parent = parent.GetParentOrNull();
            }

            var allTests = typeTests.AndTogether();

            return allTests;
        }

        private static Expression GetRuntimeTypeCheck(UnaryExpression cast, IMemberMapperData mapperData)
        {
            var getSourceCall = mapperData.GetSourceAccess(Parameters.MappingData, mapperData.SourceType);
            var rootedValue = cast.Operand.Replace(mapperData.SourceObject, getSourceCall);
            var memberHasRuntimeType = Expression.TypeIs(rootedValue, cast.Type);

            return memberHasRuntimeType;
        }

        public static SourceMemberDataSource For(IQualifiedMember sourceMember, IMemberMapperData mapperData)
        {
            sourceMember = sourceMember.RelativeTo(mapperData.SourceMember);

            var sourceMemberValue = sourceMember
                .GetQualifiedAccess(mapperData)
                .GetConversionTo(sourceMember.Type);

            var sourceMemberDataSource = new SourceMemberDataSource(sourceMember, sourceMemberValue, mapperData);

            return sourceMemberDataSource;
        }
    }
}