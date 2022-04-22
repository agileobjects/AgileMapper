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
        public static readonly IDataSource EmptyValue =
            Empty(sourceMember: null, condition: null);

        public NullDataSource(Expression value)
            : this(sourceMember: null, value, condition: null)
        {
        }

        private NullDataSource(
            IQualifiedMember sourceMember,
            Expression value,
            Expression condition)
            : base(sourceMember, EmptyParameters, value, condition)
        {
        }

        #region Factory Methods

        public static IDataSource Empty(
            IQualifiedMember sourceMember,
            Expression condition)
        {
            return new NullDataSource(
                sourceMember,
                EmptyExpression,
                condition);
        }

        #endregion

        public override bool IsValid => false;
    }
}