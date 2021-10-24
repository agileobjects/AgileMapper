namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using static Constants;

    internal class NullDataSource : DataSourceBase
    {
        public static readonly IDataSource EmptyValue = Empty(sourceMember: null);

        public NullDataSource(Expression value)
            : this(default, value)
        {
        }

        private NullDataSource(IQualifiedMember sourceMember, Expression value)
            : base(sourceMember, value)
        {
        }

        public static IDataSource Empty(IQualifiedMember sourceMember)
            => new NullDataSource(sourceMember, EmptyExpression);

        public override bool IsValid => false;
    }
}