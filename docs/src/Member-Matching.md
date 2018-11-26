By default:

* Writable fields and properties, single-parameter methods prefixed with 'Set', constructor parameters and non-null, readonly complex type or enumerable members (except arrays) are targeted
* Target members are matched to [compatibly-typed](Type-Conversion) source members by name
* Source members prefixed with 'Get' are matched without the prefix - [`GetHashCode`](https://msdn.microsoft.com/en-us/library/system.object.gethashcode%28v=vs.100%29.aspx) and [`GetType`](https://msdn.microsoft.com/en-us/library/system.object.gettype(v=vs.100).aspx) are ignored
* Members named `Id`, `<Type name>Id`, `Identifier` or `<Type name>Identifier` are matched
* Target members with no compatible source member are ignored.
* Target members with the same name as a constructor parameter are ignored (because we assume the constructor will populate them)

For example:

```C#
public class CustomerViewModel
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string HomeAddressLine1 { get; set; }
    public string WorkAddressLine1 { get; set; }
    public byte[] Data { get; set; }
}

public class Customer
{
    public string Name;
    public Guid CustomerId { get; set; }
    public Address HomeAddress { get; set; }
    public Address WorkAddress { get; }
    public void SetData(DateTime value) {}
}

public class Address
{
    public string Line1 { get; set; }
}
```

When mapping `CustomerViewModel` to `Customer`:

 - `Customer.Name` is populated using `CustomerViewModel.Name`
 - The `Customer.CustomerId` Guid is populated using the [parsed](Type-Conversion) `CustomerViewModel.Id` string
 - `Customer.HomeAddress` is populated with a new `Address` instance if one does not already exist
 - `Customer.HomeAddress.Line1` is populated using `CustomerViewModel.HomeAddressLine1`
 - `Customer.WorkAddress` is get-only, so is ignored if it's null
 - `Customer.WorkAddress.Line1` is populated using `CustomerViewModel.WorkAddressLine1`
 - `Customer.SetData(DateTime value)` is ignored, because its match - `CustomerViewModel.Data` - is a `byte[]`, which cannot be parsed to a `DateTime`

When mapping from `Customer` to `CustomerViewModel`, the matches are made in the opposite direction.