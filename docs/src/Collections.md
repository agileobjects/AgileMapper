The following collection types are supported by default:

- Arrays
- `IEnumerable`
- `IEnumerable<T>`
- `ICollection`
- `ICollection<T>`
- `List<T>`
- `IList<T>`
- `ReadOnlyCollection<T>`
- `IReadOnlyCollection<T>`
- `Collection<T>`
- `HashSet<T>`
- `ISet<T>`
- [`Dictionary<string, T>`](/Dictionary-Mapping)
- [`IDictionary<string, T>`](/Dictionary-Mapping)

Generic `List<T>` instances are created for interface-type members except `ISet<T>`, which uses a `HashSet<T>`. If a member is already populated with a non-readonly collection (e.g. an array), the existing object will be reused.

[Updates](/Performing-Updates) and [merges](/Performing-Merges) of collections of identifiable objects (i.e. Entities) are performed in an id-aware manner.

## Null Source Collections

By default, if the source collection matching a target collection is null, the target collection is populated with an empty collection. You can configure setting the target collection to null instead like this:

```cs
// Map null-source collections to null for all source
// and target types:
Mapper.WhenMapping.MapNullCollectionsToNull();

// Map null collections to null only when mapping from
// Order to OrderDto:
Mapper.WhenMapping
    .From<Order>()
    .To<OrderDto>()
    .MapNullCollectionsToNull();
```