namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using AbstractMappers;
    using TestClasses;

    internal class ManualCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            if (valueObject == null)
            {
                return null;
            }

            return new ConstructedObject(valueObject.Value);
        }
    }
}