namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringDerivedTypes
    {
        [Fact]
        public void ShouldMapACustomTypePair()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .Map<MegaProduct>()
                    .To<ProductDtoMega>();

                Product source = new MegaProduct { ProductId = "PrettyDarnMega", Price = 0.99, HowMega = 1.00m };

                var result = mapper.Map(source).ToANew<ProductDto>();

                result.ShouldBeOfType<ProductDtoMega>();
                result.ProductId.ShouldBe("PrettyDarnMega");
                result.Price.ShouldBe(0.99m);
                ((ProductDtoMega)result).HowMega.ShouldBe("1.00");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/172
        [Fact]
        public void ShouldMapACustomTypePairToAFixedDerivedTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MapTo<ProductDtoMega>();

                var source = new Product { ProductId = "12345", Price = 1.99 };

                var result = mapper.Map(source).ToANew<ProductDto>();

                result.ShouldBeOfType<ProductDtoMega>();
                result.ProductId.ShouldBe("12345");
                result.Price.ShouldBe(1.99m);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/163
        [Fact]
        public void ShouldMapACustomInterfaceTypePair()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue163.ISource>()
                    .To<Issue163.ITarget>()
                    .Map<Issue163.Source>()
                    .To<Issue163.Target>()
                    .And
                    .Map(s => s.Status, t => t.StatusId);

                Issue163.ISource source = new Issue163.Source { Status = 500 };

                var result = mapper.Map(source).ToANew<Issue163.ITarget>();

                result
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Issue163.Target>()
                    .StatusId.ShouldBe(500);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/172
        [Fact]
        public void ShouldMapACustomInterfaceTypePairToAFixedDerivedTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue163.ISource>()
                    .To<Issue163.ITarget>()
                    .MapTo<Issue163.Target>()
                    .And
                    .Map(s => s.Status, t => t.StatusId);

                Issue163.ISource source = new Issue163.Source { Status = 200 };

                var result = mapper.Map(source).ToANew<Issue163.ITarget>();

                result
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Issue163.Target>()
                    .StatusId.ShouldBe(200);
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/172
        [Fact]
        public void ShouldMapACustomInterfaceTypePairToAFixedDerivedTargetTypeMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue163.ISource>()
                    .To<PublicField<Issue163.ITarget>>()
                    .Map(ctx => ctx.Source).To(dst => dst.Value);

                mapper.WhenMapping
                    .From<Issue163.ISource>()
                    .To<Issue163.ITarget>()
                    .Map(s => s.Status, t => t.StatusId)
                    .And
                    .If(ctx => ctx.Source.Status == 404)
                    .MapTo<Issue163.Target>();
                    
                Issue163.ISource source404 = new Issue163.Source { Status = 404 };

                var result404 = mapper.Map(source404).ToANew<PublicField<Issue163.ITarget>>();

                result404
                    .ShouldNotBeNull()
                    .Value
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Issue163.Target>()
                    .StatusId.ShouldBe(404);
                    
                Issue163.ISource source503 = new Issue163.Source { Status = 503 };

                var result503 = mapper.Map(source503).ToANew<PublicField<Issue163.ITarget>>();

                result503
                    .ShouldNotBeNull()
                    .Value
                    .ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/163
        [Fact]
        public void ShouldMapACustomAbstractClassTypePair()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue163.SourceBase>()
                    .To<Issue163.TargetBase>()
                    .Map<Issue163.SourceImpl>()
                    .To<Issue163.TargetImpl>()
                    .And
                    .Map(s => s.Status, t => t.StatusId);

                Issue163.SourceBase source = new Issue163.SourceImpl { Status = 404 };

                var result = mapper.Map(source).ToANew<Issue163.TargetBase>();

                result
                    .ShouldNotBeNull()
                    .ShouldBeOfType<Issue163.TargetImpl>()
                    .StatusId.ShouldBe(404);
            }
        }

        [Fact]
        public void ShouldMapAConfiguredDerivedTypeMemberToAStruct()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .ToANew<PublicPropertyStruct<string>>()
                    .Map((mc, pps) => mc.Name)
                    .To(pps => pps.Value);

                Customer customer = new MysteryCustomer { Id = Guid.NewGuid(), Name = "Mystery!" };
                var customerResult = mapper.Map(customer).ToANew<PublicPropertyStruct<string>>();

                customerResult.Value.ShouldBe("Mystery!");
            }
        }

        [Fact]
        public void ShouldMapADerivedTypePairConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var exampleInstance = new { Name = default(string), Discount = default(decimal?), Report = default(string) };

                mapper.WhenMapping
                    .From(exampleInstance)
                    .ToANew<PersonViewModel>()
                    .If(ctx => ctx.Source.Discount.HasValue)
                    .MapTo<CustomerViewModel>()
                    .And
                    .If(ctx => !ctx.Source.Report.IsNullOrWhiteSpace())
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new
                {
                    Name = "???",
                    Discount = (decimal?).5m,
                    Report = "Lovely!"
                };

                var mysteryCustomerResult = mapper.Map(mysteryCustomerSource).ToANew<PersonViewModel>();

                mysteryCustomerResult.ShouldBeOfType<MysteryCustomerViewModel>();
                mysteryCustomerResult.Name.ShouldBe("???");
                ((CustomerViewModel)mysteryCustomerResult).Discount.ShouldBe(0.5);
                ((MysteryCustomerViewModel)mysteryCustomerResult).Report.ShouldBe("Lovely!");

                var customerSource = new
                {
                    Name = "Firsty",
                    Discount = (decimal?)1,
                    Report = string.Empty
                };

                var customerResult = mapper.Map(customerSource).ToANew<PersonViewModel>();

                customerResult.ShouldBeOfType<CustomerViewModel>();
                customerResult.Name.ShouldBe("Firsty");
                ((CustomerViewModel)customerResult).Discount.ShouldBe(1.0);

                var personSource = new
                {
                    Name = "Datey",
                    Discount = default(decimal?),
                    Report = default(string)
                };

                var personResult = mapper.Map(personSource).ToANew<PersonViewModel>();

                personResult.ShouldBeOfType<PersonViewModel>();
                personResult.Name.ShouldBe("Datey");
            }
        }

        [Fact]
        public void ShouldMapANestedDerivedTypePairConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .To<CustomerViewModel>()
                    .If((s, t) => s.Name == "Mystery Customer")
                    .MapTo<MysteryCustomerViewModel>()
                    .And
                    .If((s, t) => s.Name == "Customer Mystery!")
                    .MapTo<MysteryCustomerViewModel>();

                var mysteryCustomerSource = new PublicField<PersonViewModel>
                {
                    Value = new CustomerViewModel { Name = "Mystery Customer", Discount = 0.5 }
                };
                var result = mapper.Map(mysteryCustomerSource).ToANew<PublicProperty<PersonViewModel>>();

                result.Value.ShouldBeOfType<MysteryCustomerViewModel>().Discount.ShouldBe(0.5);

                var customerSource = new PublicField<PersonViewModel>
                {
                    Value = new CustomerViewModel { Name = "Banksy" }
                };
                result = mapper.Map(customerSource).ToANew<PublicProperty<PersonViewModel>>();

                result.Value.ShouldBeOfType<CustomerViewModel>();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/123
        [Fact]
        public void ShouldMapToAnInterfaceConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.GroupDto>()
                    .To<Issue123.Group>()
                    .Map(dto => dto.Eman, g => g.Name);

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Group)
                    .MapTo<Issue123.Group>()
                    .And
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .MapTo<Issue123.Leaf>();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Group>()
                    .Map(ctx => ctx.Source.Group).ToTarget();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Leaf>()
                    .Map(ctx => ctx.Source.Leaf).ToTarget();

                var groupDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Group,
                    Group = new Issue123.GroupDto { Eman = "composite group" }
                };

                var group = mapper.Map(groupDto).ToANew<Issue123.IComposite>() as Issue123.Group;

                group.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                group.Name.ShouldBe("composite group");
                group.Children.ShouldBeEmpty();

                var leafDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Leaf,
                    Leaf = new Issue123.LeafDto { Description = "Leaf" }
                };

                var leaf = mapper.Map(leafDto).ToANew<Issue123.IComposite>() as Issue123.Leaf;

                leaf.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                leaf.Description.ShouldBe("Leaf");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/123
        [Fact]
        public void ShouldMapToAnInterfaceInAListConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.GroupDto>()
                    .To<Issue123.Group>()
                    .Map(dto => dto.Eman, g => g.Name);

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Group)
                    .MapTo<Issue123.Group>()
                    .And
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .MapTo<Issue123.Leaf>();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Group>()
                    .Map(ctx => ctx.Source.Group).ToTarget();

                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.Leaf>()
                    .Map(ctx => ctx.Source.Leaf).ToTarget();

                var groupDto = new Issue123.GroupDto
                {
                    Eman = "outer group",
                    Children =
                    {
                        new Issue123.CompositeDto
                        {
                            Type = Issue123.CompositeType.Group,
                            Group = new Issue123.GroupDto { Eman = "inner group" }
                        },
                        new Issue123.CompositeDto
                        {
                            Type = Issue123.CompositeType.Leaf,
                            Leaf = new Issue123.LeafDto { Description = "Leaf" }
                        }
                    }
                };

                var group = mapper.Map(groupDto).ToANew<Issue123.Group>();

                group.ShouldNotBeNull();
                group.Children.Count.ShouldBe(2);

                group.Children.First().ShouldBeOfType<Issue123.Group>();
                group.Children.Second().ShouldBeOfType<Issue123.Leaf>();
            }
        }

        [Fact]
        public void ShouldUseATypedToTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue123.CompositeDto>().To<Issue123.IComposite>()
                    .If(ctx => ctx.Source.Type == Issue123.CompositeType.Leaf)
                    .Map(ctx => ctx.Source.Leaf)
                    .ToTarget<Issue123.Leaf>()
                    .AndWhenMapping
                    .From<Issue123.LeafDto>().To<Issue123.Leaf>()
                    .Map((dto, l) => dto.Description + "!")
                    .To(l => l.Description);

                var leafDto = new Issue123.CompositeDto
                {
                    Type = Issue123.CompositeType.Leaf,
                    Leaf = new Issue123.LeafDto { Description = "I am a leaf" }
                };

                var leaf = mapper.Map(leafDto).ToANew<Issue123.IComposite>() as Issue123.Leaf;

                leaf.ShouldNotBeNull();

                // ReSharper disable once PossibleNullReferenceException
                leaf.Description.ShouldBe("I am a leaf!");
            }
        }

        [Fact]
        public void ShouldUseACtorParameterWithATypedToTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue129.Source.Wrapper>()
                    .To<Issue129.Target.ITrafficObj>()
                    .If(d => d.Source.ConcreteValue == Issue129.Source.Wrapper.ConcreteValueType.Delay)
                    .Map(d => d.Source.DelayValue)
                    .ToTarget<Issue129.Target.DelayObject>();

                var delaySource = new Issue129.Source.Wrapper
                {
                    ConcreteValue = Issue129.Source.Wrapper.ConcreteValueType.Delay,
                    DelayValue = new Issue129.Source.DelayObject
                    {
                        Name = "Situation Object",
                        Duration = TimeSpan.FromHours(2).ToString()
                    }
                };

                var delayResult = mapper.Map(delaySource).ToANew<Issue129.Target.ITrafficObj>();

                delayResult.ShouldNotBeNull();
                delayResult.ShouldBeOfType<Issue129.Target.DelayObject>();

                var delayObject = (Issue129.Target.DelayObject)delayResult;
                delayObject.CurrentClass.ShouldNotBeNull();
                delayObject.CurrentClass.ShouldBeOfType<Issue129.Target.DelayClass>();

                var delayClass = (Issue129.Target.DelayClass)delayObject.CurrentClass;
                delayClass.Name.ShouldBe("Delay");
                delayClass.Duration.ShouldBe(TimeSpan.FromHours(2));
            }
        }

        [Fact]
        public void ShouldHandleANullTypedToTargetSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue129.Source.Wrapper>()
                    .To<Issue129.Target.ITrafficObj>()
                    .If(d => d.Source.ConcreteValue == Issue129.Source.Wrapper.ConcreteValueType.Delay)
                    .Map(d => d.Source.DelayValue)
                    .ToTarget<Issue129.Target.DelayObject>();

                var nullDelaySource = new Issue129.Source.Wrapper
                {
                    ConcreteValue = Issue129.Source.Wrapper.ConcreteValueType.Delay
                };

                var delayResult = mapper.Map(nullDelaySource).ToANew<Issue129.Target.ITrafficObj>();

                delayResult.ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/129
        [Fact]
        public void ShouldUseAConfiguredCtorParameterWithATypedToTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue129.Source.SituationObject>()
                    .To<Issue129.Target.SituationObject>()
                    .Map(d => d.Source.CurrentClass)
                    .ToCtor<Issue129.Target.SituationClass>();

                mapper.WhenMapping
                    .From<Issue129.Source.ActionObject>()
                    .To<Issue129.Target.ActionObject>()
                    .Map(ctx => new Issue129.Target.ActionClass())
                    .ToCtor<Issue129.Target.ActionClass>();

                mapper.WhenMapping
                    .From<Issue129.Source.Wrapper>()
                    .To<Issue129.Target.ITrafficObj>()
                    .If(d => d.Source.ConcreteValue == Issue129.Source.Wrapper.ConcreteValueType.Action)
                    .Map(d => d.Source.ActionValue)
                    .ToTarget<Issue129.Target.ActionObject>()
                    .And
                    .If(d => d.Source.ConcreteValue == Issue129.Source.Wrapper.ConcreteValueType.Situation)
                    .Map(d => d.Source.SituationValue)
                    .ToTarget<Issue129.Target.SituationObject>();

                var situationSource = new Issue129.Source.Wrapper
                {
                    ConcreteValue = Issue129.Source.Wrapper.ConcreteValueType.Situation,
                    SituationValue = new Issue129.Source.SituationObject
                    {
                        Name = "Situation Object",
                        CurrentClass = new Issue129.Source.SituationClass
                        {
                            Name = "Situation Class"
                        }
                    }
                };

                var situationResult = mapper.Map(situationSource).ToANew<Issue129.Target.ITrafficObj>();

                situationResult.ShouldNotBeNull();
                situationResult.ShouldBeOfType<Issue129.Target.SituationObject>();

                var situationObject = (Issue129.Target.SituationObject)situationResult;
                situationObject.CurrentClass.ShouldNotBeNull();
                situationObject.CurrentClass.ShouldBeOfType<Issue129.Target.SituationClass>();

                var situationClass = (Issue129.Target.SituationClass)situationObject.CurrentClass;
                situationClass.Name.ShouldBe("Situation Class");
            }
        }

        #region Helper Classes

        internal class Issue123
        {
            public enum CompositeType
            {
                Group,
                Leaf
            }

            // ReSharper disable MemberHidesStaticFromOuterClass
            public class CompositeDto
            {
                public CompositeType Type { get; set; }

                public GroupDto Group { get; set; }

                public LeafDto Leaf { get; set; }
            }
            // ReSharper restore MemberHidesStaticFromOuterClass

            public class GroupDto
            {
                public GroupDto()
                {
                    Children = new List<CompositeDto>();
                }

                public string Eman { get; set; }

                public IList<CompositeDto> Children { get; }
            }

            public class LeafDto
            {
                public string Description { get; set; }
            }

            public interface IComposite
            {
            }

            public interface IGroup : IComposite
            {
                string Name { get; }
            }

            public class Group : IGroup
            {
                public Group()
                {
                    Children = new List<IComposite>();
                }

                public string Name { get; set; }

                public IList<IComposite> Children { get; }
            }

            public interface ILeaf : IComposite
            {
                string Description { get; }
            }

            public class Leaf : ILeaf
            {
                public string Description { get; set; }
            }
        }

        internal static class Issue129
        {
            public static class Source
            {
                public class SituationClass
                {
                    public string Name { get; set; }
                }

                public class SituationObject
                {
                    public string Name { get; set; }

                    public SituationClass CurrentClass { get; set; }
                }

                public class ActionObject
                {
                    public string Name { get; set; }
                }

                public class DelayObject
                {
                    public string Name { get; set; }

                    public string Duration { get; set; }
                }

                public class Wrapper
                {
                    public enum ConcreteValueType { Situation, Action, Delay }

                    public ConcreteValueType ConcreteValue { get; set; }

                    public SituationObject SituationValue { get; set; }

                    public ActionObject ActionValue { get; set; }

                    public DelayObject DelayValue { get; set; }
                }
            }

            public static class Target
            {
                public interface ITrafficObj { ITrafficClass CurrentClass { get; } }

                public interface ITrafficClass { string Name { get; } }

                public class SituationClass : ITrafficClass
                {
                    public string Name { get; set; }
                }

                public class ActionClass : ITrafficClass
                {
                    public string Name { get; set; }
                }

                public class DelayClass : ITrafficClass
                {
                    public string Name { get; set; }

                    public TimeSpan Duration { get; set; }
                }

                public class SituationObject : ITrafficObj
                {
                    public SituationObject(SituationClass clazz) { CurrentClass = clazz; }

                    public ITrafficClass CurrentClass { get; }
                }

                public class ActionObject : ITrafficObj
                {
                    public ActionObject(ActionClass clazz) { CurrentClass = clazz; }

                    public ITrafficClass CurrentClass { get; }
                }

                public class DelayObject : ITrafficObj
                {
                    public DelayObject(TimeSpan duration)
                    {
                        CurrentClass = new DelayClass
                        {
                            Name = "Delay",
                            Duration = duration
                        };
                    }

                    public ITrafficClass CurrentClass { get; }
                }
            }
        }

        internal static class Issue163
        {
            public interface ISource
            {
                int Status { get; }
            }

            public interface ITarget
            {
                int StatusId { get; set; }
            }

            public class Source : ISource
            {
                public int Status { get; set; }
            }

            public class Target : ITarget
            {
                public int StatusId { get; set; }
            }

            public abstract class SourceBase
            {
                public int Status { get; set; }
            }

            public abstract class TargetBase
            {
                public int StatusId { get; set; }
            }

            public class SourceImpl : SourceBase
            {
            }

            public class TargetImpl : TargetBase
            {
            }
        }

        #endregion
    }
}
