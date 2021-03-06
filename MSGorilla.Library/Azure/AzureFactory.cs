﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;


namespace MSGorilla.Library.Azure
{
    public static class AzureFactory
    {
        public enum MSGorillaTable{
            Homeline,
            Userline,
            PublicSquareLine,
            TopicLine,
            EventLine,
            OwnerLine,
            AtLine,
            Reply,
            ReplyNotification,
            ReplyArchive,
            Attachment,
            RichMessage,
            MetricDataSet,
            CategoryMessage,
            Statistics,
            WordsIndex,
            SearchResults,
            SearchHistory,
            CounterSet,
            CounterRecord,
        }

        public enum MSGorillaQueue
        {
            Dispatcher,
            SearchEngineSpider,
            MailMessage
        }

        public enum MSGorillaBlobContainer
        {
            Attachment
        }

        public static CloudStorageAccount AzureStorageAccount { get; private set; }
        public static CloudStorageAccount WossStorageAccount { get; private set; }

        private static Dictionary<MSGorillaTable, string> _tableDict;
        private static Dictionary<MSGorillaQueue, string> _queueDict;
        private static Dictionary<MSGorillaBlobContainer, string> _containerDict;
        private static Dictionary<MSGorillaTable, AWCloudTable> _tableCache;

        public static string GetConnectionString(string key)
        {
            try
            {
                string connectionString = CloudConfigurationManager.GetSetting(key);
                if (string.IsNullOrEmpty(connectionString))
                {
                    var settings = ConfigurationManager.ConnectionStrings[key];
                    if (settings != null)
                    {
                        connectionString = settings.ConnectionString;
                    }
                }
                return connectionString;
            }
            catch (Exception e)
            {
                Logger.Error(e, DateTimeOffset.UtcNow, DateTime.Now, "Initial", "GetConnectionString");
                return null;
            }
            
        }

        static AzureFactory()
        {
            // init storage account
            string connectionString = GetConnectionString("StorageConnectionString");
            AzureStorageAccount = CloudStorageAccount.Parse(connectionString);
            WossStorageAccount = null;

            //check whether enable woss storage or not
            connectionString = GetConnectionString("WossStorageConnectionString");
            if (!string.IsNullOrEmpty(connectionString) &&
                !connectionString.Equals("UseDevelopmentStorage=true", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    WossStorageAccount = CloudStorageAccount.Parse(connectionString);
                    if (WossStorageAccount.Equals(AzureStorageAccount))
                    {
                        WossStorageAccount = null;
                    }
                }
                catch (Exception)
                {
                    Trace.TraceError("Fail to parse woss storage account string: " + connectionString);
                    WossStorageAccount = null;
                }                
            }

            _tableCache = new Dictionary<MSGorillaTable, AWCloudTable>();

            // init table dict
            _tableDict = new Dictionary<MSGorillaTable, string>();
            _tableDict.Add(MSGorillaTable.Homeline, "Homeline");
            _tableDict.Add(MSGorillaTable.Userline, "Userline");
            _tableDict.Add(MSGorillaTable.EventLine, "EventlineTweet");
            _tableDict.Add(MSGorillaTable.PublicSquareLine, "PublicSquareline");
            _tableDict.Add(MSGorillaTable.TopicLine, "Topicline");
            _tableDict.Add(MSGorillaTable.OwnerLine, "Ownerline");
            _tableDict.Add(MSGorillaTable.AtLine, "Atline");
            _tableDict.Add(MSGorillaTable.Reply, "Reply");
            _tableDict.Add(MSGorillaTable.ReplyNotification, "ReplyNotification");
            _tableDict.Add(MSGorillaTable.ReplyArchive, "ReplyArchive");
            _tableDict.Add(MSGorillaTable.Attachment, "Attachment");
            _tableDict.Add(MSGorillaTable.RichMessage, "RichMessage");
            _tableDict.Add(MSGorillaTable.MetricDataSet, "MetricDataSet");
            _tableDict.Add(MSGorillaTable.CategoryMessage, "CategoryMessage");
            _tableDict.Add(MSGorillaTable.Statistics, "Statistics");
            _tableDict.Add(MSGorillaTable.WordsIndex, "WordsIndex");
            _tableDict.Add(MSGorillaTable.SearchResults, "SearchResults");
            _tableDict.Add(MSGorillaTable.SearchHistory, "SearchHistory");
            _tableDict.Add(MSGorillaTable.CounterSet, "CounterSets");
            _tableDict.Add(MSGorillaTable.CounterRecord, "CounterRecord");

            // init queue dict
            _queueDict = new Dictionary<MSGorillaQueue, string>();
            _queueDict.Add(MSGorillaQueue.Dispatcher, "messagequeue");
            _queueDict.Add(MSGorillaQueue.SearchEngineSpider, "spider");
            _queueDict.Add(MSGorillaQueue.MailMessage, "mailmessage");

            // init blob container dict
            _containerDict = new Dictionary<MSGorillaBlobContainer, string>();
            _containerDict.Add(MSGorillaBlobContainer.Attachment, "attachment");
        }

        public static AWCloudTable GetTable(MSGorillaTable table)
        {
            if (_tableCache.ContainsKey(table))
            {
                return _tableCache[table];
            }

            var client = AzureStorageAccount.CreateCloudTableClient();
            CloudTable aztable = client.GetTableReference(_tableDict[table]);
            aztable.CreateIfNotExists();
            CloudTable wosstable = null;

            if (WossStorageAccount != null)
            {
                client = WossStorageAccount.CreateCloudTableClient();
                wosstable = client.GetTableReference(_tableDict[table]);
                DateTimeOffset startTime = DateTimeOffset.UtcNow;

                OperationContext opContext = new OperationContext();
                try
                {
                    wosstable.Create(null, opContext);
                }
                catch (Exception e)
                {
                    if (!(e is StorageException && e.InnerException != null && e.InnerException.Message.Equals("The remote server returned an error: (409) Conflict.")))
                    {
                        Logger.Error(e, startTime, DateTime.Now, wosstable.Uri.ToString(), "CreateTable", opContext);
                    }                        
                }
            }

            AWCloudTable awTable = new AWCloudTable(aztable, wosstable);
            _tableCache[table] = awTable;
            return awTable;
        }

        public static CloudQueue GetQueue(MSGorillaQueue queue)
        {
            var client = AzureStorageAccount.CreateCloudQueueClient();
            var azqueue = client.GetQueueReference(_queueDict[queue]);
            azqueue.CreateIfNotExists();
            return azqueue;
        }

        public static CloudBlobContainer GetBlobContainer(MSGorillaBlobContainer container)
        {
            var client = AzureStorageAccount.CreateCloudBlobClient();
            var azcontainer = client.GetContainerReference(_containerDict[container]);
            azcontainer.CreateIfNotExists();
            return azcontainer;
        }
    }
}
