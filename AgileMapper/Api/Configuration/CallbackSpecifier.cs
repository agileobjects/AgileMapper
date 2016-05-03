namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;

    public class CallbackSpecifier<TTarget>
    {
        private readonly MapperContext _mapperContext;

        internal CallbackSpecifier(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public void Call(Action<TTarget> callback)
        {
            _mapperContext.ComplexTypeFactory.AddCreationCallback(callback);
        }
    }
}