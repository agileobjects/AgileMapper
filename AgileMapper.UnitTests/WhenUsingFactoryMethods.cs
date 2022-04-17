namespace AgileObjects.AgileMapper.UnitTests
{
    using AgileMapper.Configuration;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
    public class WhenUsingFactoryMethods
    {
        [Fact]
        public void ShouldUseAParameterlessGetObjectFactoryMethod()
        {
            var source = new ParameterlessGetMethod();

            var result = Mapper.Map(source).ToANew<ParameterlessGetMethod>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("123");
        }

        [Fact]
        public void ShouldUseASingleParameterCreateObjectFactoryMethod()
        {
            var source = new PublicField<int> { Value = 456 };

            var result = Mapper.Map(source).ToANew<SingleParameterCreateMethod>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("456");
        }

        [Fact]
        public void ShouldUseGreediestFactoryMethod()
        {
            var source = new PublicTwoFieldsStruct<int, int> { Value1 = 111, Value2 = 222 };

            var result = Mapper.Map(source).ToANew<MultiParameterCreateMethod>();

            result.ShouldNotBeNull();
            result.Value1.ShouldBe("111");
            result.Value2.ShouldBe("222");
        }

        [Fact]
        public void ShouldUseFactoryMethodWithAvailableDataSources()
        {
            var source = new PublicProperty<long> { Value = 999L };

            var result = Mapper.Map(source).ToANew<MultiParameterCreateMethod>();

            result.ShouldNotBeNull();
            result.Value1.ShouldBe("999");
            result.Value2.ShouldBe("999");
        }

        [Fact]
        public void ShouldUseCtorIfMoreAvailableDataSources()
        {
            var source = new PublicTwoFields<long, long> { Value1 = 123L, Value2 = 987L };

            var result = Mapper.Map(source).ToANew<SingleParameterGetMethodMultiParameterCtor>();

            result.ShouldNotBeNull();
            result.Value1.ShouldBe("123");
            result.Value2.ShouldBe("987");
        }

        [Fact]
        public void ShouldErrorIfRedundantFactoryMethodConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFieldsStruct<string, string>>()
                        .ToANew<MultiParameterCreateMethod>()
                        .CreateInstancesUsing(ctx => MultiParameterCreateMethod.CreateObject(
                            ctx.Source.Value1,
                            ctx.Source.Value2));
                }
            });

            configEx.Message.ShouldContain(
                "MultiParameterCreateMethod.CreateObject(ctx.Source.Value1, ctx.Source.Value2)");

            configEx.Message.ShouldContain("does not need to be configured");
        }

        #region Helper Classes

        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedMember.Local
        // ReSharper disable MemberCanBePrivate.Local
        private class ParameterlessGetMethod
        {
            public static ParameterlessGetMethod GetObject()
                => new ParameterlessGetMethod { Value = "123" };

            public string Value { get; private set; }
        }

        private class SingleParameterCreateMethod
        {
            public static SingleParameterCreateMethod CreateObject(string value)
                => new SingleParameterCreateMethod { Value = value };

            public string Value { get; private set; }
        }

        private class MultiParameterCreateMethod
        {
            public static MultiParameterCreateMethod CreateObject(string value)
                => CreateObject(value, value);

            public static MultiParameterCreateMethod CreateObject(string value1, string value2)
                => new MultiParameterCreateMethod { Value1 = value1, Value2 = value2 };

            public string Value1 { get; private set; }

            public string Value2 { get; private set; }
        }

        private class SingleParameterGetMethodMultiParameterCtor
        {
            public SingleParameterGetMethodMultiParameterCtor()
            {
            }

            public SingleParameterGetMethodMultiParameterCtor(string value1, string value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public static SingleParameterGetMethodMultiParameterCtor GetObject(string value)
                => new SingleParameterGetMethodMultiParameterCtor(value, value);

            public string Value1 { get; private set; }

            public string Value2 { get; private set; }
        }

        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedMember.Global

        #endregion
    }
}
