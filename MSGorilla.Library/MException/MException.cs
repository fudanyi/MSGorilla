﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models;

namespace MSGorilla.Library.Exceptions
{
    public class MSGorillaBaseException : System.Exception
    {
        public string Description { get; set; }
        public int Code { get; set; }

        public MSGorillaBaseException()
        {
            Description = "Unknown Exception.";
            Code = 1000;
        }

        public ActionResult toActionResult()
        {
            return new ActionResult(Code, Description);
        }
    }

    public class InvalidIDException : MSGorillaBaseException
    {
        public InvalidIDException()
        {
            Description = "Invalid ID. ID should be [0-9a-zA-Z]+ .";
            Code = 1001;
        }

        public InvalidIDException(string type)
        {
            Description = string.Format("Invalid {0} ID. Please refer to http://msdn.microsoft.com/library/azure/dd179338.aspx", type);
            Code = 1001;
        }

        public InvalidIDException(string id, string type)
        {
            Description = string.Format("{0} is not a valid {1} type. Only [0-9a-zAA-Z] is recommended.");
            Code = 1001;
        }
    }

    public class UserNotFoundException : MSGorillaBaseException
    {
        public UserNotFoundException(string userid)
        {
            Description = string.Format("User {0} doesn't exist.", userid);
            Code = 2000;
        }
    }

    public class UserAlreadyExistException : MSGorillaBaseException
    {
        public UserAlreadyExistException(string userid)
        {
            Description = string.Format("Userid {0} already exists.", userid);
            Code = 2001;
        }

        public UserAlreadyExistException(string userid, string Description)
        {
            Code = 2001;
            this.Description = Description;
        }
    }

    public class LoginFailException : MSGorillaBaseException
    {
        public LoginFailException()
        {
            Description = string.Format("Login Fail. Wrong Password or Username.");
            Code = 2003;
        }
    }

    public class AccessDenyException : MSGorillaBaseException
    {
        public AccessDenyException()
        {
            Description = string.Format("Access Denied. Please Login.");
            Code = 2004;
        }
    }

    public class NoAccessToUpdatePasswordException : MSGorillaBaseException
    {
        public NoAccessToUpdatePasswordException()
        {
            Description = "Fail to update password. Changing password is only available for local account.";
            Code = 2005;
        }
    }

    public class MessageTooLongException : MSGorillaBaseException
    {
        public MessageTooLongException()
        {
            Description = "Message is too long.";
            Code = 3001;
        }
    }

    public class MessageNotFoundException : MSGorillaBaseException
    {
        public MessageNotFoundException()
        {
            Description = "The specified message doesn't exist.";
            Code = 3002;
        }
    }

    public class MessageNullException : MSGorillaBaseException
    {
        public MessageNullException()
        {
            Description = "Message can't be null";
            Code = 3003;
        }
    }

    public class InvalidMessageIDException : MSGorillaBaseException
    {
        public InvalidMessageIDException()
        {
            Description = "Invalid Message ID";
            Code = 3004;
        }
    }

    public class RichMessageTooLongException : MSGorillaBaseException
    {
        public RichMessageTooLongException()
        {
            Description = "Rich message is too long.";
            Code = 3005;
        }
    }


    //public class RetweetARetweetException : TwitterBaseException
    //{
    //    public RetweetARetweetException()
    //    {
    //        Description = "Can't retweet a retweet.";
    //        Code = 3003;
    //    }
    //}
    public class SchemaNotFoundException : MSGorillaBaseException
    {
        public SchemaNotFoundException()
        {
            Description = "The specified schema doesn't exist.";
            Code = 4001;
        }
    }

    public class SchemaAlreadyExistException : MSGorillaBaseException
    {
        public SchemaAlreadyExistException()
        {
            Description = "The specified schema already exists. Change the schema ID";
            Code = 4002;
        }
    }

    public class AttachmentNotFoundException : MSGorillaBaseException
    {
        public AttachmentNotFoundException()
        {
            Description = "Attachment Not Found.";
            Code = 5001;
        }
    }

    public class GroupAlreadyExistException : MSGorillaBaseException
    {
        public GroupAlreadyExistException(string groupID)
        {
            Description = string.Format("GroupID {0} already exists.", groupID);
            Code = 6001;
        }

        public GroupAlreadyExistException(string groupID, string Description)
        {
            Code = 6001;
            this.Description = Description;
        }
    }

    public class GroupNotExistException : MSGorillaBaseException
    {
        public GroupNotExistException()
        {
            Description = "The specified group does not exist.";
            Code = 6002;
        }
    }

    public class UnauthroizedActionException : MSGorillaBaseException
    {
        public UnauthroizedActionException()
        {
            Description = "You're not authorized to perform this action. Contact the group admin";
            Code = 6003;
        }
    }

    public class UpdateRobotMembershipException : MSGorillaBaseException
    {
        public UpdateRobotMembershipException()
        {
            Description = "You can't change a robot's membership.";
            Code = 6004;
        }
    }

    public class UpdateRobotDefaultGroupException : MSGorillaBaseException
    {
        public UpdateRobotDefaultGroupException()
        {
            Description = "You can't change a robot's default group.";
            Code = 6005;
        }
    }

    public class TopicNotFoundException : MSGorillaBaseException
    {
        public TopicNotFoundException()
        {
            Description = "The specified topic not found.";
            Code = 7001;
        }
    }

    public class MetricDataSetNotFoundException : MSGorillaBaseException
    {
        public MetricDataSetNotFoundException()
        {
            Description = "The specified metric data not found.";
            Code = 8001;
        }
    }

    public class MetricRecordKeyTooLongException : MSGorillaBaseException
    {
        public const int MaxKeyLengthInByte = 2048;
        public MetricRecordKeyTooLongException()
        {
            Description = string.Format("Metric record key length should be no long than {0} bytes.", MaxKeyLengthInByte);
            Code = 8002;
        }
    }

    public class MetricChartNotFoundException : MSGorillaBaseException
    {
        public MetricChartNotFoundException()
        {
            Description = "The specified metric chart not found.";
            Code = 8003;
        }
    }

    public class MetricGroupDismatchException : MSGorillaBaseException
    {
        public MetricGroupDismatchException()
        {
            Description = "The groups of metric chart and metric dataset are not the same.";
            Code = 8004;
        }
    }

    public class CategoryNotFoundException : MSGorillaBaseException
    {
        public CategoryNotFoundException()
        {
            Description = "The specified category doesn't exist.";
            Code = 9001;
        }
    }
    public class CategoryAlreadyExistException : MSGorillaBaseException
    {
        public CategoryAlreadyExistException(string name, string group)
        {
            Description = string.Format("A category named {0} already exist in group {1}", name, group);
            Code = 9002;
        }
    }

    public class CounterTooLargeException : MSGorillaBaseException
    {
        public CounterTooLargeException()
        {
            Description = "The size of serialized counter should not be larger than 992kb";
            Code = 10001;
        }
    }

    public class CounterSetNotFoundException : MSGorillaBaseException
    {
        public CounterSetNotFoundException()
        {
            Description = "The specified counter set not found.";
            Code = 10002;
        }
    }

    public class CounterSetAlreadyExistException : MSGorillaBaseException
    {
        public CounterSetAlreadyExistException()
        {
            Description = "The specified counter set already exist.";
            Code = 10003;
        }
    }
}
