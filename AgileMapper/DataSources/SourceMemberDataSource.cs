namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;
    using TypeConversion;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(
            IQualifiedMember sourceMember,
            Expression sourceMemberValue,
            Expression condition,
            IMemberMapperData mapperData)
            : base(
                  sourceMember,
                  mapperData.GetValueConversionOrCreation(sourceMemberValue, mapperData.TargetMember.Type),
                  mapperData)
        {
            SourceMemberTypeTest = CreateSourceMemberTypeTest(sourceMemberValue, mapperData);

            if (condition == null)
            {
                Condition = base.Condition;
                return;
            }

            Condition = IsConditional ? Expression.AndAlso(base.Condition, condition) : condition;
        }

        private static Expression CreateSourceMemberTypeTest(Expression value, IMemberMapperData mapperData)
        {
            var parent = value;
            var typeTests = default(List<Expression>);

            while (parent != mapperData.SourceObject)
            {
                if (parent.NodeType != ExpressionType.Convert)
                {
                    parent = parent.GetParentOrNull();
                    continue;
                }

                var cast = (UnaryExpression)parent;
                parent = cast.Operand;

                typeTests ??= new List<Expression>();
                typeTests.Insert(0, GetRuntimeTypeCheck(cast, mapperData));
            }

            var allTests = typeTests?.AndTogether();

            return allTests;
        }

        private static Expression GetRuntimeTypeCheck(UnaryExpression cast, IMemberMapperData mapperData)
        {
            var sourceParameter = typeof(object).GetOrCreateSourceParameter();
            var sourcePropertyIsType = Expression.TypeIs(sourceParameter, mapperData.SourceType);

            var castSource = sourceParameter.GetConversionTo(mapperData.SourceType);
            var rootedValue = cast.Operand.Replace(mapperData.SourceObject, castSource);
            var memberHasRuntimeType = Expression.TypeIs(rootedValue, cast.Type);

            return Expression.AndAlso(sourcePropertyIsType, memberHasRuntimeType);
        }

        public override Expression Condition { get; }
    }
}