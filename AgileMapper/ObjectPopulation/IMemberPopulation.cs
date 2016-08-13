namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IMemberPopulation
    {
        MemberMapperData MapperData { get; }

        bool IsSuccessful { get; }

        IEnumerable<IObjectMapper> InlineObjectMappers { get; }

        Expression GetPopulation();
    }
}