﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class AtLineEntity : TableEntity
    {
        public string Content { get; set; }

        public AtLineEntity()
        {
            ;
        }

        public AtLineEntity(string atUserid, Message msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", atUserid, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
        }
    }
}