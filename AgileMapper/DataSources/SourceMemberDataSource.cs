using System.Collections.Generic;
using System.Reflection;
using AgileObjects.AgileMapper.Extensions;

namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceMemberDataSource : DataSourceBase
    {
        public SourceMemberDataSource(IQualifiedMember sourceMember, MemberMapperData data)
            : this(sourceMember, sourceMember.GetQualifiedAccess(data.SourceObject), data)
        {
        }

        private SourceMemberDataSource(IQualifiedMember sourceMember, Expression value, MemberMapperData data)
            : base(
                  sourceMember,
                  data.MapperContext.ValueConverters.GetConversion(value, data.TargetMember.Type),
                  data)
        {
            SourceMemberTypeTest = CreateSourceMemberTypeTest(value, data);
        }

        private static Expression CreateSourceMemberTypeTest(Expression value, MemberMapperData data)
        {
            var parent = value.GetParentOrNull();
            var typeTests = new List<Expression>();

            while (parent != data.SourceObject)
            {
                if (parent.NodeType == ExpressionType.Convert)
                {
                    var cast = (UnaryExpression)parent;
                    parent = cast.Operand;

                    typeTests.Insert(0, GetRuntimeTypeCheck(cast, data));
                }

                parent = parent.GetParentOrNull();
            }

            var allTests = typeTests.AndTogether();

            return allTests;
        }

        private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource");

        private static Expression GetRuntimeTypeCheck(UnaryExpression cast, MemberMapperData data)
        {
            var getSourceCall = Expression.Call(
                Parameters.MappingData,
                _getSourceMethod.MakeGenericMethod(data.SourceType));

            var rootedValue = cast.Operand.Replace(data.SourceObject, getSourceCall);
            var memberHasRuntimeType = Expression.TypeIs(rootedValue, cast.Type);

            return memberHasRuntimeType;
        }

        public override Expression SourceMemberTypeTest { get; }
    }
}