namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal class ObjectCreationCallback
    {
        private readonly CallbackPosition _callbackPosition;
        private readonly Expression _callback;

        public ObjectCreationCallback(CallbackPosition callbackPosition, Expression callback)
        {
            _callbackPosition = callbackPosition;
            _callback = callback;
        }

        public Expression IntegrateCallback(IMemberMappingContext context)
        {
            if (_callbackPosition == CallbackPosition.After)
            {
                return Expression.IfThen(
                    Expression.NotEqual(context.TargetVariable, context.ExistingObject),
                    _callback);
            }

            return _callback;
        }
    }
}