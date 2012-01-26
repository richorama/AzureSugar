using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using NUnit.Framework;

namespace Two10.AzureSugar
{
    [TestFixture]
    public class DynamicTableTest
    {

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Test00()
        {
            var context = new DynamicTableContext(null, null);
        }

        [Test]
        public void Test01()
        {
            Credentials credentials = new Credentials("two10ra", @"dmIMUY1mg/qPeOgGmCkO333L26cNcnUA1uMcSSOFMB3cB8LkdDkh02RaYTPLBL8qMqnqazqd6uMxI2bJJEnj0g==");
            DynamicTableContext context = new DynamicTableContext("FarmConfiguration", credentials);
            foreach (IDictionary<string, object> item in context.Query("VMId eq 'CrmWebRole_IN_0'"))
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
            Credentials credentials = new Credentials("two10ra", @"dmIMUY1mg/qPeOgGmCkO333L26cNcnUA1uMcSSOFMB3cB8LkdDkh02RaYTPLBL8qMqnqazqd6uMxI2bJJEnj0g==");
            DynamicTableContext context = new DynamicTableContext("FarmConfiguration", credentials);
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
            Credentials credentials = new Credentials("two10ra", @"dmIMUY1mg/qPeOgGmCkO333L26cNcnUA1uMcSSOFMB3cB8LkdDkh02RaYTPLBL8qMqnqazqd6uMxI2bJJEnj0g==");
            DynamicTableContext context = new DynamicTableContext("FarmConfiguration", credentials);
            dynamic item = new { PartitionKey = "1", RowKey = Guid.NewGuid().ToString(), Value = "Hello World" };
            context.InsertOrReplace(item);

        }

        [Test]
        public void Test05()
        {
            Credentials credentials = new Credentials("two10ra", @"dmIMUY1mg/qPeOgGmCkO333L26cNcnUA1uMcSSOFMB3cB8LkdDkh02RaYTPLBL8qMqnqazqd6uMxI2bJJEnj0g==");
            DynamicTableContext context = new DynamicTableContext("FarmConfiguration", credentials);
            dynamic item = new ExpandoObject();
            item.PartitionKey = "1";
            item.RowKey = Guid.NewGuid().ToString();
            item.Value = "Hello World";
            context.Insert(item);
        }

        [Test]
        public void Test06()
        {
            Credentials credentials = new Credentials("two10ra", @"dmIMUY1mg/qPeOgGmCkO333L26cNcnUA1uMcSSOFMB3cB8LkdDkh02RaYTPLBL8qMqnqazqd6uMxI2bJJEnj0g==");
            DynamicTableContext context = new DynamicTableContext("FarmConfiguration", credentials);
            dynamic item = new ExpandoObject();
            item.PartitionKey = "1";
            item.RowKey = Guid.NewGuid().ToString();
            item.Value = "Hello World";
            context.InsertOrMerge(item);
        }
    }
}
