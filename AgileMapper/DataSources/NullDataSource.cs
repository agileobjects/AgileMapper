namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class NullDataSource : DataSourceBase
    {
        public NullDataSource(Expression value)
            : base(default(IQualifiedMember), value)
        {
        }

        public override bool IsValid => false;
    }
}