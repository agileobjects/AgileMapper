namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ObjectCreationCallback
    {
        private readonly CallbackPosition _callbackPosition;
        private readonly Expression _callback;
        private readonly IList<Expression> _conditions;

        public ObjectCreationCallback(
            CallbackPosition callbackPosition,
            Expression callback,
            Expression condition)
        {
            _callbackPosition = callbackPosition;
            _callback = callback;
            _conditions = new List<Expression>();

            if (condition != null)
            {
                _conditions.Add(condition);
            }
        }

        public Expression IntegrateCallback(IObjectMappingContext omc)
        {
            if (_callbackPosition == CallbackPosition.After)
            {
                _conditions.Insert(0, omc.CreatedObject.GetIsNotDefaultComparison());
            }

            if (_conditions.Any())
            {
                return Expression.IfThen(_conditions.GetIsNotDefaultComparisonsOrNull(), _callback);
            }

            return _callback;
        }
    }
}