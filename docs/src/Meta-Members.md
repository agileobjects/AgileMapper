With the following Types:

```cs
class Account
{
    public int Id { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Address> DeliveryAddresses { get; set; }
}

class AccountDto
{
    public int Id { get; set; }
    public bool HasOrders { get; set; }
    public int DeliveryAddressCount { get; set; }
    public bool FirstDeliveryAddressHasPostcode { get; set; }
}
```

`HasOrders`, `DeliveryAddressCount` and `FirstDeliveryAddressHasPostcode` are 'meta' members - members which contain information about other members. AgileMapper will automatically populate them as follows:

- `HasOrders` - whether `Orders` is non-null and has a count greater than zero

- `DeliveryAddressCount` - the number of elements in the source `DeliveryAddresses` collection

- `FirstDeliveryAddressHasPostcode` - whether the first element in the source `DeliveryAddresses` collection has a non-default `Postcode` property value

Meta members are also supported in [query projections](/query-projection).

## Types of Meta Member

### Has&lt;MemberName&gt;

Whether the member with name &lt;MemberName&gt;:

- Has a non-default value, if it is a value type
- Is non-null and contains elements, if it is an enumerable type
- Is non-null otherwise

### &lt;MemberName&gt;Count / NumberOf&lt;MemberNames&gt;

The number of elements contained by the enumerable member with name &lt;MemberNames&gt;. &lt;MemberName&gt;Count expects the enumerable member to be named with the plural of &lt;MemberName&gt;, _e.g._ `OrderCount` will look for an enumerable named `Orders`.

### First&lt;MemberName&gt; / Last&lt;MemberName&gt;

The first or last elements contained by the enumerable member with name &lt;MemberNames&gt;. If the enumerable member is null or empty, the default value will be used. Both expect the enumerable member to be named with the plural of &lt;MemberName&gt;, _e.g._ `FirstDeliveryAddress` will look for an enumerable named `DeliveryAddresses`.

## Composite Meta Members

Meta members can be combined, as with `FirstDeliveryAddressHasPostcode`, above. 

## Limitations

Navigation via one member only is supported, _e.g._ `HasDeliveryAddresses` only supports a member named `DeliveryAddresses`, not a complex type member named `Delivery` with a nested complex type member named `Addresses`.