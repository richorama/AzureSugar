using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.WindowsAzure.StorageClient.Protocol;

namespace Two10.AzureSugar
{

    public class DynamicTableContext
    {
        private Credentials credentials;

        private string tableName;

        public DynamicTableContext(string tableName, Credentials credentials)
        {
            if (tableName == null) throw new ArgumentNullException("tableName");
            if (credentials == null) throw new ArgumentNullException("credentials");
            this.credentials = credentials;
            this.tableName = tableName;
        }

        public IEnumerable<dynamic> Query(string queryString)
        {
            var webRequest = BuildRequest(string.Format(@"http://{0}.table.core.windows.net/{1}()?$filter={2}", credentials.AccountName, tableName, queryString.Replace(' ', '+')));
            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            var response = webRequest.GetResponse();

            foreach (var item in ParseResponse(response))
            {
                yield return item;
            }

            yield break;
        }

        public dynamic Get(string partitionKey, string rowKey)
        {
            var webRequest = BuildRequest(string.Format(@"http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", credentials.AccountName, tableName, partitionKey, rowKey));
            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            try
            {
                var response = webRequest.GetResponse();
                foreach (var item in ParseResponse(response))
                {
                    // return the first item in the result set
                    return item;
                }
            }
            catch (WebException)
            {
                // the server will return a 404 if the entity does not exist
                return null;
            }
            return null;
        }

        public void InsertOrReplace(dynamic entity)
        {
            IDictionary<string, object> dictionary = ParseObject(entity);
            var uri = string.Format(@"http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", credentials.AccountName, tableName, dictionary["PartitionKey"], dictionary["RowKey"]);
            Write(dictionary, uri, "PUT");
        }

        public void Insert(dynamic entity)
        {
            IDictionary<string, object> dictionary = ParseObject(entity);
            var uri = string.Format(@"http://{0}.table.core.windows.net/{1}", credentials.AccountName, tableName, dictionary["PartitionKey"], dictionary["RowKey"]);
            this.Write(dictionary, uri, "POST");
        }

        public void InsertOrMerge(dynamic entity)
        {
            IDictionary<string, object> dictionary = ParseObject(entity);
            var uri = string.Format(@"http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", credentials.AccountName, tableName, dictionary["PartitionKey"], dictionary["RowKey"]);
            this.Write(dictionary, uri, "MERGE");
        }

        private void Write(IDictionary<string, object> entity, string uri, string method)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!entity.ContainsKey("PartitionKey")) throw new ArgumentException("Entity has no PartitionKey");
            if (!entity.ContainsKey("RowKey")) throw new ArgumentException("Entity has no RowKey");

            var webRequest = BuildRequest(uri, method);
            webRequest.ContentType = @"application/atom+xml";

            if (!entity.ContainsKey("Timestamp"))
            {
                entity.Add("Timestamp", "0001-01-01T00:00:00");
            }

            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            WriteToRequestStream(entity, uri, webRequest);

            var response = webRequest.GetResponse();

        }

        private IDictionary<string, object> ParseObject(dynamic value)
        {
            if (value == null) return new Dictionary<string, object>();
            if (value is IDictionary<string, object>) return value;
            var dictionary = new Dictionary<string, object>();
            foreach (var prop in value.GetType().GetProperties())
            {
                dictionary.Add(prop.Name, prop.GetValue(value, null));
            }
            return dictionary;
        }

        private static void WriteToRequestStream(IDictionary<string, object> dictionary, string uri, HttpWebRequest webRequest)
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version='1.0' encoding='utf-8' standalone='yes'?><entry xmlns:d='http://schemas.microsoft.com/ado/2007/08/dataservices' xmlns:m='http://schemas.microsoft.com/ado/2007/08/dataservices/metadata' xmlns='http://www.w3.org/2005/Atom'><title /><updated>2009-03-18T11:48:34.9840639-07:00</updated><author><name /></author><id>");
            sb.Append(uri);
            sb.Append(@"</id><content type='application/xml'><m:properties>");
            foreach (var key in dictionary.Keys)
            {
                if (key == "Timestamp")
                    sb.Append(string.Format("<d:{0} m:type='Edm.DateTime'>{1}</d:{0}>", key, dictionary[key]));
                else
                    sb.Append(string.Format("<d:{0}>{1}</d:{0}>", key, dictionary[key]));
            }
            sb.Append(@"</m:properties></content></entry>");
            string body = sb.ToString();
            webRequest.ContentLength = body.Length;

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(body);
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }
        }


        private HttpWebRequest BuildRequest(string uri, string method = "GET")
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = method;
            webRequest.Headers.Add("DataServiceVersion", "2.0;NetFx");
            webRequest.Headers.Add("MaxDataServiceVersion", "2.0;NetFx");
            webRequest.Headers.Add("x-ms-version", "2011-08-18");
            return webRequest;
        }

        private static IEnumerable<dynamic> ParseResponse(WebResponse response)
        {
            XDocument x = XDocument.Load(response.GetResponseStream());

            foreach (var entry in x.Descendants(XName.Get("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")))
            {
                IDictionary<string, object> dictionary = new ExpandoObject();
                foreach (var item in entry.Elements())
                {
                    dictionary.Add(item.Name.LocalName, item.Value);
                }
                yield return dictionary;
            }
        }

    }
}
