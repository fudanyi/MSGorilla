﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library;
using MSGorilla.Library.DAL;
using MSGorilla.Library.Azure;

namespace MSGorilla.StatusReporter
{
    public class StatusCollector
    {
        public int GetCurrentUserCount()
        {
            int count = 0;
            using (var _gorillaCtx = new MSGorillaContext())
            {
                count = _gorillaCtx.Database.SqlQuery<int>("select count(*) from userprofile where password is null").First();
            }
            Logger.Info(string.Format("{0} common user exist.", count));
            return count;
        }

        public int GetCurrentRobotCount()
        {
            List<string> robots = GetCurrentRobotID();
            return robots.Count;
        }

        public int GetCurrentTopicCount()
        {
            int count = 0;

            using (var _gorillaCtx = new MSGorillaContext())
            {
                count = _gorillaCtx.Database.SqlQuery<int>("select count(*) from topic").First();
            }
            Logger.Info(string.Format("{0} topics exist.", count));
            return count;
        }

        public List<string> GetCurrentRobotID()
        {
            List<string> userid = new List<string>();

            using (var _gorillaCtx = new MSGorillaContext())
            {
                userid = _gorillaCtx.Database.SqlQuery<string>("select userid from userprofile where password is not null").ToList();
            }

            userid.Remove("user1");
            userid.Remove("user2");
            userid.Remove("user3");
            userid.Remove("user4");
            userid.Remove("fdy");
            userid.Remove("bin");
            userid.Remove("ShareAccount");
            userid.Remove("tanmayw");
            return userid;
        }

        public int GetUserPostMessageCountByDateUtc(string userid, DateTime date)
        {
            CloudTable userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(date)));

            TableQuery tableQuery = new TableQuery().Where(query);

            int count = userline.ExecuteQuery(tableQuery).Count();
            Logger.Debug(string.Format("{0} post {1} messages in {2:yyyy-MM-dd}", userid, count, date.ToUniversalTime().Date));
            return count;
        }

        public int GetTotalRobotMessageCountByDateUtc(DateTime date)
        {
            int count = 0;
            List<string> robitID = GetCurrentRobotID();

            CloudTable userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            foreach (string userid in robitID)
            {
                try
                {
                    count += GetUserPostMessageCountByDateUtc(userid, date);
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }

            Logger.Info(string.Format("Robots posted {0} messages totally in {1:yyyy-MM-dd}", count, date.ToUniversalTime()));
            return count;
        }

        public HistoryData LoadDataByDateUtc(DateTime date, HistoryDataContext ctx = null)
        {
            string sql = "select * from HistoryData where Date >= {0} and Date < {1}";
            date = date.ToUniversalTime();
            date = DateTime.Parse(string.Format("{0:yyyy-MM-dd}", date));

            HistoryData data = null;

            if(ctx == null){
                using (ctx = new HistoryDataContext())
                {
                    data = ctx.HistoryMonitoringData.
                        SqlQuery(sql, date, date.AddDays(1)).
                        FirstOrDefault();
                }
            }else{
                data = ctx.HistoryMonitoringData.
                        SqlQuery(sql, date, date.AddDays(1)).
                        FirstOrDefault();
            }

            return data;
        }

        public HistoryData GetLatestData(HistoryDataContext ctx = null)
        {
            string sql = "select top 1 * from HistoryData order by Date desc";
            if (ctx == null)
            {
                using (ctx = new HistoryDataContext())
                {
                    return ctx.HistoryMonitoringData.SqlQuery(sql).FirstOrDefault();
                }
            }
            else
            {
                return ctx.HistoryMonitoringData.SqlQuery(sql).FirstOrDefault();
            }
        }

        public List<HistoryData> LoadDataByDateUtc(DateTime start, DateTime end, HistoryDataContext ctx = null)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            string sql = "select * from HistoryData where Date >= {0} and Date < {1}";

            if (ctx == null)
            {
                using (ctx = new HistoryDataContext())
                {
                    return ctx.HistoryMonitoringData.SqlQuery(sql, start, end).ToList();
                }
            }
            else
            {
                return ctx.HistoryMonitoringData.SqlQuery(sql, start, end).ToList();
            }
        }

        public void UpdateStatusAndSave()
        {
            using (HistoryDataContext ctx = new HistoryDataContext())
            {
                DateTime current = DateTime.UtcNow;
                DateTime today = current;// DateTime.Parse(string.Format("{0:yyyy-MM-ddZ}", DateTime.UtcNow));
                DateTime yesterday = today.AddDays(-1);

                HistoryData dataOfYesterday = LoadDataByDateUtc(current.AddDays(-1), ctx);
                if (dataOfYesterday != null)
                {
                    dataOfYesterday.CountOfMsgPostedByRobot = this.GetTotalRobotMessageCountByDateUtc(yesterday);
                    ctx.SaveChanges();
                }
                else
                {
                    HistoryData data = new HistoryData();
                    data.Date = yesterday;
                    data.CountOfMsgPostedByRobot = GetTotalRobotMessageCountByDateUtc(yesterday);
                    ctx.HistoryMonitoringData.Add(data);
                    ctx.SaveChanges();
                }

                HistoryData dataOfToday = LoadDataByDateUtc(current, ctx);
                if (dataOfToday == null)
                {
                    HistoryData data = new HistoryData();
                    data.Date = DateTime.UtcNow;
                    data.UserCount = this.GetCurrentUserCount();
                    data.RobotCount = this.GetCurrentRobotCount();
                    data.TopicCount = this.GetCurrentTopicCount();
                    data.CountOfMsgPostedByRobot = 0;

                    ctx.HistoryMonitoringData.Add(data);
                    ctx.SaveChanges();
                }                
            }
        }
    }
}
