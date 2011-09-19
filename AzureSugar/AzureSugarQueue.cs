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
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class AzureSugarQueue<T>
    {
        public string QueueName { get; private set; }

        public CloudStorageAccount Account { get; private set; }

        public CloudQueue Queue { get; private set; }

        public AzureSugarQueue(CloudStorageAccount account)
        {
            this.Account = account;
            this.QueueName = typeof(T).Name.ToLower();
            this.Queue = account.CreateCloudQueueClient().GetQueueReference(this.QueueName);
        }

        public AzureSugarQueue(CloudStorageAccount account, string name)
        {
            this.Account = account;
            this.QueueName = name;
            this.Queue = account.CreateCloudQueueClient().GetQueueReference(this.QueueName);
        }

        public void CreateQueue()
        {
            this.Queue.CreateIfNotExist();
        }

        public AzureSugarMessage<T> Pop()
        {
            return this.Pop(true);
        }

        public AzureSugarMessage<T> Pop(bool commitOnDispose)
        {
            var message = this.Queue.GetMessage();
            if (null != message)
            {
                return new AzureSugarMessage<T>(this, message, commitOnDispose);
            }
            return null;
        }

        public void Push(T value)
        {
            string serializedValue = Serialize<T>(value);
            this.Queue.AddMessage(new CloudQueueMessage(serializedValue));
        }

        private static string Serialize<Y>(Y objectModel)
        {
            if (objectModel == null) return string.Empty;

            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, settings))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);

                    XmlSerializer serializer = new XmlSerializer(typeof(Y));
                    serializer.Serialize(xmlWriter, objectModel, ns);

                    return sw.ToString();
                }
            }
        }

        public System.Collections.Generic.IEnumerable<T> AsQueryable()
        {
            do
            {
                using (var message = this.Pop())
                {
                    if (null == message)
                    {
                        yield break;
                    }

                    yield return message.Content;
                }
            }
            while (true);
        }

    }


}
