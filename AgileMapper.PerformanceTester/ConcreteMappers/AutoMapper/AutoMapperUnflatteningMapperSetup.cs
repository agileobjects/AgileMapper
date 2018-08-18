﻿namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Flattening;

    internal class AutoMapperUnflatteningMapperSetup : UnflatteningMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void Reset()
        {
            Mapper.Reset();
        }

        protected override void SetupUnflatteningMapper()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<ModelObject, ModelDto>().ReverseMap());

            Mapper.Map<ModelDto, ModelObject>(new ModelDto());
        }
    }
}