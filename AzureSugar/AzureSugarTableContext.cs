#region Copyright (c) 2011 two10 degrees
//
// (C) Copyright 2011 two10 degrees
//      All rights reserved.
//
// This software is provided "as is" without warranty of any kind,
// express or implied, including but not limited to warranties as to
// quality and fitness for a particular purpose. Active Web Solutions Ltd
// does not support the Software, nor does it warrant that the Software
// will meet your requirements or that the operation of the Software will
// be uninterrupted or error free or that any defects will be
// corrected. Nothing in this statement is intended to limit or exclude
// any liability for personal injury or death caused by the negligence of
// Active Web Solutions Ltd, its employees, contractors or agents.
//
#endregion

namespace Two10.AzureSugar
{
    using System;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class TableNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public TableNameAttribute(string name)
        {
            this.Name = name;
        }
    }


    public class AzureSugarTableContext : IDisposable
    {
        public TableServiceContext Context { get; private set; }

        public CloudStorageAccount Account { get; private set; }

        public bool CommitOnDispose { get; private set; }

        public AzureSugarTableContext(CloudStorageAccount account)
        {
            this.Context = new TableServiceContext(account.TableEndpoint.ToString(), account.Credentials);
            this.Account = account;
            this.CommitOnDispose = true;
        }

        public AzureSugarTableContext(CloudStorageAccount account, bool commitOnDispose)
        {
            this.Context = new TableServiceContext(account.TableEndpoint.ToString(), account.Credentials);
            this.Account = account;
            this.CommitOnDispose = commitOnDispose;
        }

        public T Create<T>() where T : TableServiceEntity, new()
        {
            return this.Create<T>("_default");
        }

        public T Create<T>(string partitionKey) where T : TableServiceEntity, new()
        {
            T t = new T();
            t.PartitionKey = partitionKey;
            t.RowKey = Guid.NewGuid().ToString();
            this.Context.AddObject(this.GetTableName<T>(), t);
            return t;
        }

        public IQueryable<T> Query<T>()
        {
            return this.Context.CreateQuery<T>(this.GetTableName<T>());
        }

        public void Dispose()
        {
            this.Context.SaveChanges();
        }

        public string GetTableName<T>()
        {
            Type type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(TableNameAttribute), true);
            if (null != attributes && attributes.Length > 0)
            {
                return (attributes[0] as TableNameAttribute).Name;
            }
            return type.Name;
        }


        public void CreateTable<T>()
        {
            try
            {
                var client = this.Account.CreateCloudTableClient();
                client.CreateTable(this.GetTableName<T>());
            }
            catch (StorageClientException)
            {

            }
        }

        public void VoteCommit()
        {
            this.CommitOnDispose = true;
        }

    }


}
