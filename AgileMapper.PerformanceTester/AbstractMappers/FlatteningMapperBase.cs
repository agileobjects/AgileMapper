namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using TestClasses;

    internal abstract class FlatteningMapperBase : IObjectMapper
    {
        public string Name => GetType().Name;

        public abstract void Initialise();

        public object Map()
        {
            return Flatten(new ModelObject
            {
                BaseDate = new DateTime(2007, 4, 5),
                Sub = new ModelSubObject
                {
                    ProperName = "Some name",
                    SubSub = new ModelSubSubObject
                    {
                        CoolProperty = "Cool daddy-o"
                    }
                },
                Sub2 = new ModelSubObject
                {
                    ProperName = "Sub 2 name"
                },
                SubWithExtraName = new ModelSubObject
                {
                    ProperName = "Some other name"
                },
            });
        }

        protected abstract ModelDto Flatten(ModelObject model);
    }
}