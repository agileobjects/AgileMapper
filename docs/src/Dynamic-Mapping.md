AgileMapper's [dictionary mapping](/Dictionary-Mapping) also applies to [ExpandoObject](https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.expandoobject?view=netframework-4.7.1)s, which are mapped using their `IDictionary<string, object>` interface.

Out of the box:

* Member names must match target member names exactly, ignoring case ([configurable](/configuration/Dynamic-Mapping#configuring-member-names))
* Parent and child member names are matched to ExpandoObject member names separated with an underscore ([configurable](/configuration/Dynamic-Mapping#configuring-separators)), or flattened - with no separator
* Enumerable elements are matched to ExpandoObject member names by their index separated by underscores ([configurable](/configuration/Dynamic-Mapping#configuring-element-indexes))
* ExpandoObjects can contain all or a mixture of value type values, collections and complex types - anything with a matching member name is used
* Target members with no matching member in the ExpandoObject are ignored

### Mapping From An ExpandoObject

For example, the following target type:

```cs
public class Doctor
{
    public string Name { get; set; }
    public string[] PhoneNumbers { get; set; }
    public IReadOnlyCollection<Specialty> Specialties { get; set; }
}
public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

...can be mapped from the following source ExpandoObject:

```cs
dynamic source = new ExpandoObject();
source.Name = "Andy";
source.PhoneNumbers_0 = "01234 567890";
source.PhoneNumbers_1 = "07890 654321";
source.Specialties_0_Id = 123;
source.Specialties_0_Name = "Emergency Medicine";
source.Specialties_1_Id = 456;
source.Specialties_1_Name = "Critical Care";

var doctor = Mapper.Map(source).ToANew<Doctor>();
```

The created `doctor` will have the following property values:

* `Name` set to 'Andy'
* `PhoneNumbers` set to a new, 2-element string array containing:
    * "01234 567890" and
    * "07890 654321"
* `Addresses` set to a new, 2-element `List<Specialty>` containing a `Specialty`:
    * With `Id` set to '123'
    * With `Name` set to "Emergency Medicine", and another `Specialty`:
    * With `Id` set to '456'
    * With `Name` set to "Critical Care"

Note that the elements of the second `Specialty` have no separator, but the mapping works anyway.

### Mapping To An ExpandoObject

The following source `Doctor`:

```cs
var source = new Doctor
{
    Name = "Bob",
    PhoneNumbers = new[] { "01234 567890", "07890 123456" },
    Specialties = new[]
    {
        new Specialty { Id = 123, Name = "Emergency Medicine" }
    }
};
```

...can be mapped to an ExpandoObject:

```cs
dynamic expando = Mapper.Map(source).ToANew<ExpandoObject>();
// or:
dynamic result = Mapper.Map(source).ToANew<dynamic>();
```

The created `ExpandoObject` (in both cases) will have the following member names and values:

* `"Name"` set to "Bob"
* `"PhoneNumbers_0"` set to "01234 567890"
* `"PhoneNumbers_1"` set to "07890 123456"
* `"Specialties_0_Id"` set to 123
* `"Specialties_0_Name"` set to "Emergency Medicine"

## .NET Standard 1.0

.NET Standard 1.0 does not support mapping dynamic root objects - it can only map nested dynamic members.
.NET Standard 1.3 successfully maps both scenarios.

## Configuration

Dynamic mapping is [highly configurable](/configuration/Dynamic-Mapping).