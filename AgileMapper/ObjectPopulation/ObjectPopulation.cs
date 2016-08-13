namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ObjectPopulation
    {
        public ObjectPopulation(Expression action)
            : this(new[] { action }, Enumerable.Empty<IObjectMapper>())
        {
        }

        public ObjectPopulation(IEnumerable<Expression> actions, IEnumerable<IObjectMapper> inlineObjectMappers)
        {
            Actions = actions;
            InlineObjectMappers = inlineObjectMappers;
        }

        public IEnumerable<Expression> Actions { get; }

        public IEnumerable<IObjectMapper> InlineObjectMappers { get; }
    }
}