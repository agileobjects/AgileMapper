namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(IMemberMapperData mapperData)
            : this(mapperData.SourceMember, mapperData)
        {
        }

        public SourceMemberDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
            : this(
                  sourceMember,
                  sourceMember.GetQualifiedAccess(mapperData.SourceObject).GetConversionTo(sourceMember.Type),
                  mapperData)
        {
        }

        private SourceMemberDataSource(IQualifiedMember sourceMember, Expression value, IMemberMapperData mapperData)
            : base(
                  sourceMember,
                  mapperData.MapperContext.ValueConverters.GetConversion(value, mapperData.TargetMember.Type),
                  mapperData)
        {
            SourceMemberTypeTest = CreateSourceMemberTypeTest(value, mapperData);
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

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource");

        private static Expression GetRuntimeTypeCheck(UnaryExpression cast, IMemberMapperData mapperData)
        {
            var getSourceCall = Expression.Call(
                Parameters.MappingData,
                _getSourceMethod.MakeGenericMethod(mapperData.SourceType));

            var rootedValue = cast.Operand.Replace(mapperData.SourceObject, getSourceCall);
            var memberHasRuntimeType = Expression.TypeIs(rootedValue, cast.Type);

            return memberHasRuntimeType;
        }
    }
}