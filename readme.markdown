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
    	var customer = context.Create<Customer>();
    	customer.Firstname = "John";
    	customer.Lastname = "Smith";
    }

Primary keys (GUIDs) are automatially assigned. All commits are performed on disposal of the context. 

It's just as easy to query the table:

    using (var context = new AzureSugarTableContext(CloudStorageAccount.DevelopmentStorageAccount))
    {
        foreach (var customer in (from c in context.Query<Customer>() where c.Firstname == "John" select c))
        {
            Console.WriteLine(customer.Firstname);
        }
    }

Queues
------

Queues are strongly typed. Let's say we are working with this class.

    public class Foo
    {
        public string Bar { get; set; }
        public string Baz { get; set; }
    }
	
To push a message on to the queue, just do this:

    var foo = new Foo { Bar = "bar", Baz = "baz" };
    var queue = new AzureSugarQueue<Foo>(CloudStorageAccount.DevelopmentStorageAccount);
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

Queues also support IEnumerable, so you can apply linq expressions to the queue, and iterate over the messages:

    foreach (var foo in queue.AsQueryable().Take(10).OrderBy(f => f.Bar))
    {
        Console.WriteLine(foo.Bar);
    }

The queue name is automatically derived from the type name 'Foo', however you can override this as well.

About
-----
AzureSugar was written by Richard Astbury. For more information, or Azure consultancy, please contact two10 degrees: http://www.two10degrees.com/ 
