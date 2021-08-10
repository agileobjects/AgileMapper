## Add the Package

Add the AgileMapper.Buildable package to the project which will contain your configuration (see
below), and generate your mappers when built:

```shell
PM> Install-Package AgileObjects.AgileMapper.Buildable -Pre
```
The `-Pre` flag is required as AgileMapper.Buildable is currently in preview.

## Add Configuration

To specify which mapper source code should be generated, add one or more 
[configuration classes](../configuration/classes) to a project, derived from AgileMapper.Buildable's
`BuildableMapperConfiguration` class:

```cs
public class ProductMappingConfiguration : BuildableMapperConfiguration
{
    protected override void Configure()
    {
        // Configure default Mapper ProductDto -> Product mapping:
        WhenMapping
            .From<ProductDto>()
            .To<Product>()
            .Map((dto, p) => dto.Spec)
            .To(p => p.Specification)
            .And
            .Ignore(p => p.Price, p => p.CatNum);

        // Generate all Product -> ProductDto mappers:
        GetPlansFor<Product>().To<ProductDto>();
    }
}
```

AgileMapper.Buildable configuration classes work the same as 
[AgileMapper configuration classes](../configuration/classes), but instead of configuring mappers
built at runtime, they configure mappers generated at build time.

AgileMapper.Buildable auto-discovers its configuration, and does not need it to be registered using
the `Mapper.WhenMapping.UseConfigurations.From*` methods.

## Generate Mappers

When the project is built, it will generate the following source code based on your configuration:

- One instance-scoped mapper per configured source type
- A static `Mapper` class
- A set of mapping extension methods.

These files are output to a `Mappers` folder and namespace at the root of the project. The static
mapper and extension methods use the instance mappers to perform mappings.

The generated static Mapper and extension methods have the same signatures as AgileMapper's
equivalent classes, so you can switch from AgileMapper's mapping to your generated mappers simply by
changing the `using AgileObjects.AgileMapper;` namespace import to 
`using [YourProject.Namespace].Mappers;`.