Mappers can be cloned, to enable 'derived' mappers to inherit, add to and override a 'root' configuration:

``` C#
var baseMapper = Mapper.CreateNew();

// Setup the base configuration:
baseMapper.WhenMapping
    .From<Order>()
    .To<OrderDto>()
    .Map((o, dto) => o.ProdId)
    .To(dto => dto.ProductId)
    .And
    .Map((o, dto) => o.Id)
    .To(dto => dto.OrderNumber);

// Clone a new mapper for mapping UK orders:
var ukOrderMapper = baseMapper.CloneSelf();

// Setup UK-specific configuration:
ukOrderMapper.WhenMapping
    .To<OrderDto>()
    .Map(ctx => DateTime.UtcNow.AddHours(1))
    .To(dto => dto.DateCreated);

// Clone a new mapper for mapping US orders:
var usOrderMapper = baseMapper.CloneSelf();

// Setup US-specific configuration:
usOrderMapper.WhenMapping
    .To<OrderDto>()
    .Map(ctx => DateTime.UtcNow.AddHours(-4))
    .To(dto => dto.DateCreated);
```