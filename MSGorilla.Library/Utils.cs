﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.Library
{
    public class Utils
    {
        public static string MD5Encoding(string rawPass)
        {
            if (string.IsNullOrEmpty(rawPass))
            {
                return "d41d8cd98f00b204e9800998ecf8427e";
            }

            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hs)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string EncodeBase64(string code)
        {
            string encode = "";
            byte[] bytes = Encoding.UTF8.GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

        public static string ToAzureStorageSecondBasedString(DateTime timestamp, bool toUtc = true)
        {
            if (toUtc)
            {
                timestamp = timestamp.ToUniversalTime();
            }
            return ((long)DateTime.MaxValue.Subtract(timestamp).TotalMilliseconds).ToString("D15");
            //return timestamp.ToUniversalTime().ToString("yyyyMMddHHmmss");
        }

        public static string ToAzureStorageDayBasedString(DateTime timestamp, bool toUtc = true)
        {
            if (toUtc)
            {
                timestamp = timestamp.ToUniversalTime();
            }
            return ((long)DateTime.MaxValue.Subtract(timestamp).TotalDays).ToString("D7");
            //return timestamp.ToUniversalTime().ToString("yyyyMMdd");
        }

        public static string ToAzureStorageHourBasedString(DateTime timestamp, bool toUtc = true)
        {
            if (toUtc)
            {
                timestamp = timestamp.ToUniversalTime();
            }
            return ((long)DateTime.MaxValue.Subtract(timestamp).TotalHours).ToString();
            //return timestamp.ToUniversalTime().ToString("yyyyMMdd");
        }

        public static string NextKeyString(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                return "";
            }

            char[] array = current.ToCharArray();
            array[array.Length - 1]++;
            return new string(array);
        }

        public static bool IsValidID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (id.Contains('\\') ||
                id.Contains('|') ||
                id.Contains(' ') ||
                id.Contains('@') ||
                id.Contains('?') ||
                id.Contains('/') ||
                id.Contains('#') ||
                id.Contains('\t') ||
                id.Contains('\n') ||
                id.Contains('\r'))
            {
                return false;
            }

            return true;
        }

        public static string Token2String(TableContinuationToken token)
        {
            if (token == null)
            {
                return null;
            }
            string ret = (token.NextPartitionKey == null ? "" : token.NextPartitionKey) + ";";
            ret += (token.NextRowKey == null ? "" : token.NextRowKey) + ";";
            ret += (token.NextTableName == null ? "" : token.NextTableName) + ";";
            ret += (token.TargetLocation == StorageLocation.Primary ? "Primary" : "Secondary") + ";";
            return ret;
        }

        public static TableContinuationToken String2Token(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            string[] parts = token.Split(';');
            if (parts.Length < 4)
            {
                return null;
            }
            TableContinuationToken ret = new TableContinuationToken();
            ret.NextPartitionKey = parts[0];
            ret.NextRowKey = parts[1];
            ret.NextTableName = string.IsNullOrEmpty(parts[2]) ? null : parts[2];
            ret.TargetLocation = "Primary".Equals(parts[3]) ? StorageLocation.Primary : StorageLocation.Secondary;
            return ret;
        }

        public static UserProfile CreateNewUser(string userid, string displayName = null, string description = null)
        {
            if (!IsValidID(userid))
            {
                throw new InvalidIDException();
            }
            UserProfile userprofile = new UserProfile();
            userprofile.Userid = userprofile.DisplayName = userid;
            if (displayName != null)
            {
                userprofile.DisplayName = displayName;
            }
            if (description != null)
            {
                userprofile.Description = description;
            }
            userprofile.Password = null;
            userprofile.FollowersCount = userprofile.FollowingsCount = 0;
            userprofile.IsRobot = false;
            userprofile.DefaultGroup = "microsoft";
            return userprofile;
        }

        public static List<String> GetAtUserid(string message)
        {
            List<string> AtUser = new List<string>();
            if (string.IsNullOrEmpty(message))
            {
                return AtUser;
            }

            Regex r = new Regex(@"(\s|^)@([0-9a-z\-]+)(?=\s|$)", RegexOptions.IgnoreCase);
            MatchCollection matches = r.Matches(message);

            foreach (Match m in matches)
            {
                string u = m.Value.Trim();
                AtUser.Add(u.Substring(1));
            }

            return AtUser;
        }

        public static List<String> GetTopicNames(string message)
        {
            List<string> topicNames = new List<string>();
            if (string.IsNullOrEmpty(message))
            {
                return topicNames;
            }

            Regex r = new Regex(@"(\s|^)#([\w][\w \-\.,:&\*\+]*)?[\w]#(?=\s|$)", RegexOptions.IgnoreCase);
            //Regex r = new Regex(@"#(([\w \-]+(#{2})?)*[\w \-]+)#(\s|$)", RegexOptions.IgnoreCase);
            MatchCollection matches = r.Matches(message);

            foreach (Match m in matches)
            {
                string t = m.Value.Trim();
                topicNames.Add(t.Substring(1, t.Length - 2));
            }

            return topicNames;
        }

        public static string StringArray2String(string[] array)
        {
            if (array == null || array.Length == 0)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < (array.Length - 1); i++)
            {
                sb.Append(array[i]);
                sb.Append("|");
            }
            sb.Append(array[array.Length - 1]);
            return sb.ToString();
        }

        public static string[] String2StringArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return str.Split('|');
        }

        public static string Array2String<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < (array.Length - 1); i++)
            {
                sb.Append(array[i].ToString());
                sb.Append("|");
            }
            sb.Append(array[array.Length - 1]);
            return sb.ToString();
        }

        public static int[] String2IntArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new int[0];
            }

            var l = str.Split('|');
            List<int> a = new List<int>();
            foreach (var s in l)
            {
                a.Add(Convert.ToInt32(s));
            }

            return a.ToArray();
        }

        public delegate T String2T<T>(string s);
        public static T[] String2Array<T>(string str, String2T<T> convert)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new T[0];
            }

            var l = str.Split('|');
            List<T> a = new List<T>();
            foreach (var s in l)
            {
                a.Add(convert(s));
            }

            return a.ToArray<T>();
        }

        public static string Txt2Html(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = text.Replace("&", "&#38;");
            text = text.Replace("<", "&#60;");
            text = text.Replace(">", "&#62;");
            text = text.Replace("\"", "&#34;");
            text = text.Replace(" ", "&nbsp;");
            text = text.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
            text = text.Replace("\r\n", "<br/>");
            return text;
        }

        public static string GetDownloadAttachmentUri(string attachmentID)
        {
            return string.Format("/api/attachment/download?attachmentID={0}", attachmentID);
        }

        public static byte[] Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return base64EncodedBytes;
        }

        public static string CaseInsenstiveReplace(string originalString, string oldValue, string newValue)
        {
            Regex regEx = new Regex(oldValue,
               RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regEx.Replace(originalString, newValue);
        }

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static bool IsNullOrEmptyOrNone(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return true;
            }
            if (str.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }


        public const int BlockSize = 32 * 1024;
        /// <summary>
        /// Max attached size is 31*32kb
        /// return success or fail
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static bool AttachBin2Entity(DynamicTableEntity entity, byte[] rawData)
        {
            if (rawData == null)
            {
                return true;
            }

            if (rawData.Length > 31 * BlockSize)
            {
                return false;
            }

            entity.Properties["BinSize"] = new EntityProperty(rawData.Length);
            int blockCount = (rawData.Length - 1) / BlockSize + 1;
            for (int i = 0; i < blockCount; i++)
            {
                int size = 0;
                if (i == (blockCount - 1))
                {
                    size = rawData.Length % BlockSize;
                }
                else
                {
                    size = BlockSize;
                }
                byte[] temp = new byte[size];
                Array.Copy(rawData, BlockSize * i, temp, 0, size);
                entity.Properties["BinBlock" + i] = new EntityProperty(temp);
            }

            return true;
        }

        public static byte[] GetBinFromEntity(DynamicTableEntity entity)
        {
            if(entity == null || 
                !entity.Properties.ContainsKey("BinSize") || 
                entity.Properties["BinSize"].Int32Value < 0)
            {
                return null;
            }

            int size = entity.Properties["BinSize"].Int32Value.Value;
            byte[] rawBytes = new byte[size];

            int blockCount = (size - 1) / BlockSize + 1;
            for (int i = 0; i < blockCount; i++)
            {
                int blockSize = 0;
                if (i == (blockCount - 1))
                {
                    blockSize = size % BlockSize;
                }
                else
                {
                    blockSize = BlockSize;
                }
                Array.Copy(entity.Properties["BinBlock" + i].BinaryValue, 0, rawBytes, i * BlockSize, blockSize);
            }

            return rawBytes;
        }
    }
}
