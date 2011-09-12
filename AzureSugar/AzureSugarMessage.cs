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
    using System.IO;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure.StorageClient;

    public class AzureSugarMessage<T> : IDisposable
    {

        public bool CommitOnDispose { get; private set; }

        public AzureSugarMessage(AzureSugarQueue<T> queue, CloudQueueMessage message, bool commitOnDispose)
        {
            this.Queue = queue;
            this.Message = message;
            this.CommitOnDispose = commitOnDispose;
        }

        public T Content
        {
            get
            {
                return DeserializeString<T>(this.Message.AsString);
            }
        }

        private static T DeserializeString<T>(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return default(T);

            using (StringReader sr = new StringReader(value))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(sr);
            }
        }

        public AzureSugarQueue<T> Queue { get; private set; }

        public CloudQueueMessage Message { get; private set; }

        public void VoteCommit()
        {
            this.CommitOnDispose = true;
        }

        public void Dispose()
        {
            if (CommitOnDispose && null != this.Queue && null != this.Message)
            {
                this.Queue.Queue.DeleteMessage(this.Message);
            }
        }
    }
}
