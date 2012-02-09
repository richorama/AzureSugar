using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using NUnit.Framework;

namespace Two10.AzureSugar
{
    [TestFixture]
    public class DynamicTableTest
    {

        // NOTE THAT THE DEV STORE WILL NOT WORK
        const string ACCOUNT_NAME = "xxx";
        const string KEY = @"yyy";
        const string TABLE_NAME = "Table1";

        [Test]
        public void Setup()
        {
            var account = new CloudStorageAccount(new StorageCredentialsAccountAndKey(ACCOUNT_NAME, KEY), false);
            var tableClient = account.CreateCloudTableClient();
            tableClient.CreateTableIfNotExist(TABLE_NAME);
            var context = new DynamicTableContext(TABLE_NAME, new Credentials(ACCOUNT_NAME, KEY));
            context.Insert(new { PartitionKey = "1", RowKey = "1", Value1 = "TEST" });
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Test00()
        {
            var context = new DynamicTableContext(null, null);
        }

        [Test]
        public void Test01()
        {
            Credentials credentials = new Credentials(ACCOUNT_NAME, KEY);
            DynamicTableContext context = new DynamicTableContext(TABLE_NAME, credentials);
            foreach (IDictionary<string, object> item in context.Query("Value1 eq 'TEST'"))
            {
                foreach (string key in item.Keys)
                {
                    Console.WriteLine(key + " = " + item[key]);
                }
            }
        }

        [Test]
        public void Test02()
        {
            Credentials credentials = new Credentials(ACCOUNT_NAME, KEY);
            DynamicTableContext context = new DynamicTableContext(TABLE_NAME, credentials);
            foreach (IDictionary<string, object> item in context.Get("1", "1"))
            {
                foreach (string key in item.Keys)
                {
                    Console.WriteLine(key + " = " + item[key]);
                }
            }
        }

        [Test]
        public void Test04()
        {
            Credentials credentials = new Credentials(ACCOUNT_NAME, KEY);
            DynamicTableContext context = new DynamicTableContext(TABLE_NAME, credentials);
            dynamic item = new { PartitionKey = "1", RowKey = Guid.NewGuid().ToString(), Value = "Hello World" };
            context.InsertOrReplace(item);

        }

        [Test]
        public void Test05()
        {
            Credentials credentials = new Credentials(ACCOUNT_NAME, KEY);
            DynamicTableContext context = new DynamicTableContext(TABLE_NAME, credentials);
            dynamic item = new ExpandoObject();
            item.PartitionKey = "1";
            item.RowKey = Guid.NewGuid().ToString();
            item.Value = "Hello World";
            context.Insert(item);
        }

        [Test]
        public void Test06()
        {
            Credentials credentials = new Credentials(ACCOUNT_NAME, KEY);
            DynamicTableContext context = new DynamicTableContext(TABLE_NAME, credentials);
            dynamic item = new ExpandoObject();
            item.PartitionKey = "1";
            item.RowKey = Guid.NewGuid().ToString();
            item.Value = "Hello World";
            context.InsertOrMerge(item);
        }
    }
}
