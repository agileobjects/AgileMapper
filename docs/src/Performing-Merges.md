Update an object's unpopulated members with values from another using:

```cs
Mapper.Map(customerViewModel).OnTo(customer);
```

When merging collections, objects are matched by id ([configurable](/configuration/Object-Identifiers) if necessary). For example:

```cs
var source = new Collection<CustomerViewModel>
{
    new CustomerViewModel { Id = 1,    Name = "Rod" },
    new CustomerViewModel { Id = 2,    Name = "Jane" },
    new CustomerViewModel { Id = null, Name = "Freddy" }
};
var target = List<Customer>
{
    new Customer { CustomerId = 2, Name = null },
    new Customer { CustomerId = 1, Name = "Bungle" },
    new Customer { CustomerId = 3, Name = "Zippy" }
};
var result = Mapper.Map(source).OnTo(target);
```

In this case:

* `source[0]` is matched to `target[1]`, but `target[1].Name` is already populated, so it isn't changed
* `source[1]` is matched to `target[0]`, so `target[0].Name` is changed from `null` to 'Jane'
* `source[2]` has no match in the target collection, so a new `Customer` is added with `CustomerId` set to `null` and `Name` set to 'Freddy'
* `target[2]` (`CustomerId` = 3) has no match in the `source` collection, so it isn't changed
* The original `List<Customer>` instance is maintained and assigned to the `result` variable