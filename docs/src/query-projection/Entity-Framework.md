AgileMapper contains version-specific support for Entity Framework 5, [Entity Framework 6](https://docs.microsoft.com/en-us/ef/ef6), and [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core), extending each version's ability to support [query projection](/query-projection).

### Type Conversion

The following conversions are supported, along with the same projections from and / or to Nullable types:

| Projection from | Projection to | EF Core                | EF 6                   | EF 5                   |
| --------------- | ------------- |:----------------------:|:----------------------:|:----------------------:|
| Bool            | String        |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| DateTime        | String *      |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| Numeric         | Bool          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| Numeric         | Enum **       |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| Numeric         | String ***    |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| String          | Bool          |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| String          | DateTime      |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| String          | Enum **       |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|
| String          | Guid          |<ul><li>- [x] </li></ul>|                        |                        |
| String          | Numeric       |<ul><li>- [x] </li></ul>|                        |                        |

\* In DateTime to string conversion:
- EF Core uses `CultureInfo.CurrentCulture.DateTimeFormat`
- EF 6 uses the data store's date format
- EF 5 uses [SqlFunctions.DatePart](https://msdn.microsoft.com/en-us/library/dd487171(v=vs.110).aspx) to project to [yyyy-%M-%d %H:%m:%s](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)

\** Flags enums are not supported

\*** In decimal and double to-string conversion, EF 5 only includes the first two decimal places

### Projection Features

| Feature                | EF Core                | EF 6                   | EF 5                   |
| ---------------------- |:----------------------:|:----------------------:|:----------------------:|
| Derived Types *        |<ul><li>- [x] </li></ul>|                        |                        |
| Recursion **           |<ul><li>- [x] </li></ul>|                        |                        |
| Collection Members *** |<ul><li>- [x] </li></ul>|<ul><li>- [x] </li></ul>|                        |

\* EF5 and EF6 do not support casting a projected Type to its base Type, so do not support projecting to [conditional derived types](/query-projection/Derived-Types).

\** EF5 and EF6 do not support creating instances of a projected Type with [differing numbers](https://stackoverflow.com/questions/39139402/the-type-appears-in-two-structurally-incompatible-initializations-within-a-singl) of member initialisations in the same query, so do not support projecting [recursive relationships](/query-projection/Recursive-Relationships).

\*** EF5 can only project to `IEnumerable{T}` members, not `ICollection{T}`, `List{T}`, `T[]`, etc.