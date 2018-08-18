﻿namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Ctor;

    internal class AutoMapperCtorMapper : CtorMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ValueObject, ConstructedObject>();
            });

            _mapper = config.CreateMapper();
        }

        protected override ConstructedObject Construct(ValueObject valueObject) 
            => _mapper.Map<ValueObject, ConstructedObject>(valueObject);
    }
}