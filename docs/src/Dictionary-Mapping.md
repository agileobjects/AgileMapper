AgileMapper has extensive support for mapping to and from Dictionaries. Out of the box:

* Only dictionaries with string keys are supported
* Dictionary keys must match target member names exactly, ignoring case ([configurable](Dictionary-Mapping-Configuration#configuring-keys))
* Parent and child member names are matched to dictionary keys separated with a dot ([configurable](Dictionary-Mapping-Configuration#configuring-separators)), or flattened - with no separator
* Enumerable elements are matched to dictionary keys by their index inside square brackets ([configurable](Dictionary-Mapping-Configuration#configuring-element-indexes))
* Dictionaries can contain all or a mixture of value type values, collections and complex types - anything with a matching key is used
* Target members with no matching key in the dictionary are ignored

### Mapping From A Dictionary

For example, the following target type:

```cs
public class ContactDetails
{
    public string Name { get; set; }
    public string[] PhoneNumbers { get; set; }
    public IEnumerable<Address> Addresses { get; set; }
}
public class Address
{
    public int HouseNumber { get; set; }
    public string StreetName { get; set; }
}
```

...can be mapped from the following source dictionary:

```cs
var source = new Dictionary<string, string>
{
    ["Name"] = "Steve",
    ["PhoneNumbers[0]"] = "01234 567890",
    ["PhoneNumbers[1]"] = "07890 654321",
    ["PhoneNumbers[2]"] = "01234 987654",
    ["Addresses[0].HouseNumber"] = "123",
    ["Addresses[0]StreetName"] = "Dictionary Street"
};

var contactDetails = Mapper.Map(source).ToANew<ContactDetails>();
```

The created `contactDetails` will have the following property values:

* `Name` set to 'Steve'
* `PhoneNumbers` set to a new, 3-element string array containing:
    * "01234 567890"
    * "07890 654321" and
    * "01234 987654"
* `Addresses` set to a new, 1-element `List<Address>` containing an `Address`:
    * With `HouseNumber` set to '123' ([parsed](Type-Conversion) from the string)
    * With `StreetName` set to "Dictionary Street"

Note that the `StreetName` key had no separator, but the mapping works anyway.

### Mapping To A Dictionary

The following source `ContactDetails`:

```cs
var source = new ContactDetails
{
    Name = "Bob",
    PhoneNumbers = new[] { "01234 567890", "07890 123456" },
    Addresses = new[]
    {
        new Address { HouseNumber = 123, StreetName = "My Street" },
        new Address { HouseNumber = 456, StreetName = "Your Street" }
    }
};
```

...can be mapped to a dictionary:

```cs
var dictionary = Mapper.Map(source).ToANew<Dictionary<string, object>>();
// or:
var dictionaryInterface = Mapper.Map(source).ToANew<IDictionary<string, object>>();
```

The created `Dictionary` will have the following keys and values:

* `"Name"` set to "Bob"
* `"PhoneNumbers[0]"` set to "01234 567890"
* `"PhoneNumbers[1]"` set to "07890 123456"
* `"Addresses[0].HouseNumber"` set to 123
* `"Addresses[0].StreetName"` set to "My Street"
* `"Addresses[1].HouseNumber"` set to 456
* `"Addresses[1].StreetName"` set to "Your Street"

## Configuration

Dictionary mapping is [highly configurable](Dictionary-Mapping-Configuration).