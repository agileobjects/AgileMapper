namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using TestClasses;
    using Xunit;

    public class WhenViewingMappingPlans
    {
        [Fact]
        public void ShouldApplyAnExpressionConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<Person>()
                    .Over<PersonViewModel>(cfg => cfg
                        .Map((p, pvm) => p.Title + " " + p.Name)
                        .To(pvm => pvm.Name));

                plan.ShouldContain("pToPvmData.Target.Name = sourcePerson.Title + \" \" + sourcePerson.Name");

                var result = mapper
                    .Map(new Person { Title = Title.Count, Name = "Dooko" })
                    .Over(new PersonViewModel());

                result.Name.ShouldBe("Count Dooko");
            }
        }

        [Fact]
        public void ShouldApplyAnIgnoredMemberConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<Person>()
                    .ToANew<PersonViewModel>(cfg => cfg
                        .Ignore(pvm => pvm.AddressLine1));

                plan.ShouldContain("// AddressLine1 is ignored");

                var result = mapper
                    .Map(new Customer { Name = "Luke", Address = new Address { Line1 = "Far, Far Away" } })
                    .ToANew<PersonViewModel>();

                result.Name.ShouldBe("Luke");
                result.AddressLine1.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldCombineApiAndInlineConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Customer>()
                    .OnTo<CustomerViewModel>()
                    .Map((c, cvm) => c.Title + " " + c.Name)
                    .To(cvm => cvm.Name);

                string plan = mapper
                    .GetPlanFor<Customer>()
                    .OnTo<CustomerViewModel>(cfg => cfg
                        .Ignore(cvm => cvm.AddressLine1));

                plan.ShouldContain("cToCvmData.Target.Name = sourceCustomer.Title + \" \" + sourceCustomer.Name");
                plan.ShouldContain("// AddressLine1 is ignored");

                var result = mapper
                    .Map(new Customer
                    {
                        Title = Title.Dr,
                        Name = "Vader",
                        Address = new Address { Line1 = "Far, Far Away" }
                    })
                    .OnTo(new CustomerViewModel());

                result.Name.ShouldBe("Dr Vader");
                result.AddressLine1.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyMemberFilterExpressionsConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlanFor<Address>()
                    .ToANew<Address>(cfg => cfg
                        .IgnoreTargetMembersWhere(m => m.IsPropertyMatching(p => p.Name == "Line2")));

                plan.ShouldContain("m.IsPropertyMatching(p => p.Name == \"Line2\")");

                var result = mapper
                    .DeepClone(new Customer { Address = new Address { Line1 = "1", Line2 = "2" } });

                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("1");
                result.Address.Line2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldApplyInlinePlansConfigurationToAllRuleSets()
        {
            using (var mapper = Mapper.CreateNew())
            {
                string plan = mapper
                    .GetPlansFor<Product>()
                    .To<ProductDto>(cfg => cfg
                        .Map((p, dto) => p.ProductId + " DTO")
                        .To(dto => dto.ProductId));

                plan.ShouldContain("ProductId + \" DTO\"");

                var source = new Product { ProductId = "BOOM" };

                var createResult = mapper.Map(source).ToANew<ProductDto>();
                var updateResult = mapper.Map(source).Over(new ProductDto { ProductId = "ID!" });
                var mergeResult = mapper.Map(source).OnTo(new ProductDto());

                createResult.ProductId.ShouldBe("BOOM DTO");
                updateResult.ProductId.ShouldBe("BOOM DTO");
                mergeResult.ProductId.ShouldBe("BOOM DTO");
            }
        }

        [Fact]
        public void ShouldErrorIfConflictingDataSourcesConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper
                        .GetPlansFor<Product>()
                        .To<ProductDto>(cfg => cfg
                            .Map((p, dto) => p.ProductId + " DTO")
                            .To(dto => dto.ProductId)
                            .And
                            .Map((p, dto) => p.ProductId + " DTO!")
                            .To(dto => dto.ProductId));
                });

                configEx.Message.ShouldContain("already has a configured data source");
            }
        }

        [Fact]
        public void ShouldErrorIfDuplicateIgnoredMembersConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var configEx = Should.Throw<MappingConfigurationException>(() =>
                {
                    mapper.WhenMapping
                        .From<Product>().To<ProductDto>()
                        .Ignore(dto => dto.ProductId);

                    mapper
                        .GetPlanFor<Product>()
                        .ToANew<ProductDto>(cfg => cfg
                            .Ignore(dto => dto.ProductId));
                });

                configEx.Message.ShouldContain("has already been ignored");
            }
        }
    }
}
