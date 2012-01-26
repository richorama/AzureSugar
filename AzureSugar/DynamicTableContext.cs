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

        public IEnumerable<dynamic> Get(string partitionKey, string rowKey)
        {
            var webRequest = BuildRequest(string.Format(@"http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", credentials.AccountName, tableName, partitionKey, rowKey));
            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            var response = webRequest.GetResponse();

            foreach (var item in ParseResponse(response))
            {
                yield return item;
            }

            yield break;
        }

        public void InsertOrReplace(dynamic entry)
        {
            if (null == entry) throw new ArgumentNullException("entry");
            var dictionary = entry as IDictionary<String, Object>;

            if (null == dictionary) throw new ApplicationException("dynamic object should be an ExpandoObject");
            if (!dictionary.ContainsKey("PartitionKey")) throw new ApplicationException("No PartitionKey");
            if (!dictionary.ContainsKey("RowKey")) throw new ApplicationException("No RowKey");

            var uri = string.Format(@"http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", credentials.AccountName, tableName, entry.PartitionKey, entry.RowKey);

            var webRequest = BuildRequest(uri);
            webRequest.Method = "PUT";
            webRequest.ContentType = @"application/atom+xml";

            if (!dictionary.ContainsKey("Timestamp"))
            {
                dictionary.Add("Timestamp", "0001-01-01T00:00:00");
            }

            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            WriteToRequestStream(dictionary, uri, webRequest);

            var response = webRequest.GetResponse();
        }

        public void Insert(dynamic entry)
        {
            if (null == entry) throw new ArgumentNullException("entry");
            var dictionary = entry as IDictionary<String, Object>;

            if (null == dictionary) throw new ApplicationException("dynamic object should be an ExpandoObject");
            if (!dictionary.ContainsKey("PartitionKey")) throw new ApplicationException("No PartitionKey");
            if (!dictionary.ContainsKey("RowKey")) throw new ApplicationException("No RowKey");

            var uri = string.Format(@"http://{0}.table.core.windows.net/{1}", credentials.AccountName, tableName, entry.PartitionKey, entry.RowKey);
            var webRequest = BuildRequest(uri);
            webRequest.Method = "POST";
            webRequest.ContentType = @"application/atom+xml";


            if (!dictionary.ContainsKey("Timestamp"))
            {
                dictionary.Add("Timestamp", "0001-01-01T00:00:00");
            }

            TableRequest.SignRequestForSharedKeyLite(webRequest, credentials);
            WriteToRequestStream(dictionary, uri, webRequest);

            var response = webRequest.GetResponse();

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


        private HttpWebRequest BuildRequest(string uri)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = "GET";
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
