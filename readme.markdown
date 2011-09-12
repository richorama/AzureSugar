AzureSugar
==========

A lightweight .NET library which makes working with the Azure API easier.

Table Storage
-------------

Define a class which represents your table, and optionally supply a name using the 'TableName' attribute.

    [TableName("Customers")]  // <- this is optional
    public class Customer : TableServiceEntity
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }

The AzureSugarTableContext allows you to create new customers easily:

    using (var context = new AzureSugarTableContext(CloudStorageAccount.DevelopmentStorageAccount))
    {
    	Customer customer = context.Create<Customer>();
    	customer.Firstname = "John";
    	customer.Lastname = "Smith";
    }

All commits are performed on disposal of the context. It's just as easy to query the table:

    using (var context = new AzureSugarTableContext(CloudStorageAccount.DevelopmentStorageAccount))
    {
		foreach (Customer c in (from c in context.Query<Customer>() select c))
		{
			Console.WriteLine(c.Firstname);
		}
    }

Queues
------

Queues are strongly typed, to make them easier to work with. Let's say we are working with this class.

    public class Foo
    {
        public string Bar { get; set; }
        public string Baz { get; set; }
    }
	
To push a message on to the queue, just do this:

    var queue = new AzureSugarQueue<Foo>(CloudStorageAccount.DevelopmentStorageAccount);
    Foo foo = new Foo { Bar = "bar", Baz = "baz" };
    queue.Push(foo);

To pop a message from a queue, we just need to do this:

    var queue = new AzureSugarQueue<Foo>(CloudStorageAccount.DevelopmentStorageAccount);
    using (var message = queue.Pop())
    {
        Foo foo = message.Content;
		// do something with foo
    }

Your object is automatically deleted from the queue on the disposal of the message, however, you can have more control over this:

    using (var message = queue.Pop(false))
    {
        var foo2 = message.Content;
        message.VoteCommit();
    }

In this case, the message will only be deleted if 'VoteCommit' is called.

The queue name is automatically derived from the type name 'Foo', however you can override this as well.




