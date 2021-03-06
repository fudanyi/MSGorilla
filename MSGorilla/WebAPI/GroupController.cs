﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Http;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Models;

using MSGorilla.Utility;

namespace MSGorilla.WebAPI
{
    public class GroupController : BaseController
    {
        private GroupManager _groupManager = new GroupManager();

        public static DisplayGroup GetDisplayGroup(string groupID, string userid, string role = null)
        {
            string key = CacheHelper.DisplayGroupPrefix + userid + "@" + groupID;
            if (CacheHelper.Contains(key))
            {
                return CacheHelper.Get<DisplayGroup>(key);
            }

            DisplayGroup result = null;
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = new GroupManager().GetGroupByID(groupID, _gorillaCtx);
                if (!string.IsNullOrEmpty(role))
                {
                    result = new DisplayGroup(group, role);
                }
                else
                {
                    result = new DisplayGroup(group, userid, _gorillaCtx);
                }
            }

            CacheHelper.Add(key, result);
            return result;
        }

        /// <summary>
        /// Create a new Group. You'll be the default admin of the group
        /// 
        /// Example output:
        /// {
        ///     "GroupID": "microsoft",
        ///     "DisplayName": "Microsoft",
        ///     "Description": "default group for all active users",
        ///     "IsOpen": true,
        ///     "Status":"Joined"
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <param name="displayName">display group name</param>
        /// <param name="description">description of the group</param>
        /// <param name="isOpen">If the group is open, userid do not need admin's permission to join the group</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayGroup CreateGroup(string groupID, string displayName = null, string description = null, bool isOpen = false)
        {
            string me = whoami();

            if(string.IsNullOrEmpty(displayName)){
                displayName = groupID;
            }
            Group group =  _groupManager.CreateGroup(me, groupID, displayName, description, isOpen);

            MembershipHelper.RefreshJoinedGroup(me);
            return GetDisplayGroup(groupID, me, "admin");
        }

        /// <summary>
        /// Find a Group by id.
        /// 
        /// Example output:
        /// {
        ///     "GroupID": "microsoft",
        ///     "DisplayName": "Microsoft",
        ///     "Description": "default group for all active users",
        ///     "IsOpen": true,
        ///     "Status":"Joined"
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayGroup GetGroup(string groupID)
        {
            string me = whoami();
            //Group group = _groupManager.GetGroupByID(groupID);
            //if (group == null)
            //{
            //    throw new GroupNotExistException();
            //}
            return GetDisplayGroup(groupID, me); 
        }

        /// <summary>
        /// get all existing groups
        /// 
        /// example output:
        /// [
        ///     {
        ///         "GroupID": "microsoft",
        ///         "DisplayName": "MicrosoftALL",
        ///         "Description": "default group for all user",
        ///         "IsOpen": true,
        ///         "Status":"Joined"
        ///     },
        ///     ......
        ///     {
        ///         "GroupID": "woss",
        ///         "DisplayName": "WOSS",
        ///         "Description": "Woss Team",
        ///         "IsOpen": false,
        ///         "Status":"Not Joined"
        ///     }
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayGroup> GetAllGroup()
        {
            string me = whoami();
            List<DisplayGroup> result = new List<DisplayGroup>();
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                foreach (Group g in _groupManager.GetAllGroup(_gorillaCtx))
                {
                    result.Add(new DisplayGroup(g, me, _gorillaCtx));
                }
                return result;
            }            
        }

        /// <summary>
        /// Update Group info. Only group admin can do this.
        /// 
        /// example output:
        /// {
        ///     "GroupID": "msgorilladev",
        ///     "DisplayName": "MSGorilla Dev",
        ///     "Description": "MSgorilla Devs and Testers",
        ///     "IsOpen": false,
        ///     "Status":"Joined"
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <param name="displayname">group display name</param>
        /// <param name="description">group description</param>
        /// <param name="isOpen">Is open group? User can join a open group without approval</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayGroup UpdateGroup(string groupID, string displayname = null, string description = null, bool isOpen = false)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);

            Group group = new Group();
            group.GroupID = groupID;
            group.DisplayName = displayname;
            group.Description = description;
            group.IsOpen = isOpen;
            return new DisplayGroup(_groupManager.UpdateGroup(me, group), me, new MSGorillaEntities()); 
        }

        /// <summary>
        /// Join a open group.
        /// 
        /// example output:
        /// {
        ///     "GroupID": "woss",
        ///     "MemberID": "user1",
        ///     "Role": "user"
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public ActionResult JoinGroup(string groupID)
        {
            string me = whoami();
            Membership member = _groupManager.JoinGroup(whoami(), groupID);
            MembershipHelper.RefreshJoinedGroup(me);
            return new ActionResult();
        }

        /// <summary>
        /// Admin can add a user into his group
        /// 
        /// example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "Success."
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut]
        public ActionResult AddMember(string userid, string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            Membership member = _groupManager.AddMember(groupID, userid);
            MembershipHelper.RefreshJoinedGroup(userid);
            return ActionResult.Success();
        }

        /// <summary>
        /// Admin remove a user from a group
        /// 
        /// example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "Success."
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpDelete]
        public ActionResult RemoveMember(string userid, string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            _groupManager.RemoveMember(groupID, whoami(), userid);
            return ActionResult.Success();
        }

        /// <summary>
        /// Admin modify user role in his group. 
        /// Only role of a corpnet user can be changed to admin or user.
        /// Robot role is not updatable.
        /// 
        /// example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "Success."
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <param name="userid">user id</param>
        /// <param name="role">Can be "admin" or "user"</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut]
        public ActionResult UpdateMembership(string groupID, string userid, string role)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            _groupManager.UpdateMembership(groupID, whoami(), userid, role);

            return ActionResult.Success();
        }

        /// <summary>
        /// get all group member
        /// 
        /// example output:
        /// [
        ///     {
        ///         "GroupID": "woss",
        ///         "User": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test",
        ///             "FollowingsCount": 13,
        ///             "FollowersCount": 6,
        ///             "MessageCount": 211,
        ///             "IsFollowing": 1
        ///         },
        ///         "Role": "admin"
        ///     },
        ///     ......
        ///     {
        ///         "GroupID": "woss",
        ///         "User": {
        ///             "Userid": "user3",
        ///             "DisplayName": "User3",
        ///             "PortraitUrl": null,
        ///             "Description": "test user3",
        ///             "FollowingsCount": 2,
        ///             "FollowersCount": 2,
        ///             "MessageCount": 6,
        ///             "IsFollowing": 0
        ///         },
        ///         "Role": "user"
        ///     }
        /// ]
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet]
        public List<MembershipView> GetAllGroupMember(string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(groupID, me);
            //List<MembershipView> result = new List<MembershipView>();
            //foreach(Membership member in _groupManager.GetAllGroupMember(groupID))
            //{
            //    result.Add(member);
            //}
            //return result;
            return _groupManager.GetGroupMembershipView(groupID, me);
        }

        /// <summary>
        /// get all group that I've joined
        /// 
        /// example output:
        /// [
        ///     {
        ///         "GroupID": "microsoft",
        ///         "DisplayName": "MicrosoftALL",
        ///         "Description": "default group for all user",
        ///         "IsOpen": true,
        ///         "MemberID": "user1",
        ///         "Role": "user"
        ///     },
        /// 	......
        ///     {
        ///         "GroupID": "msgorilladev",
        ///         "DisplayName": "MSGorilla Dev",
        ///         "Description": "MSgorilla Devs and Testers",
        ///         "IsOpen": false,
        ///         "MemberID": "user1",
        ///         "Role": "admin"
        ///     }
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMembership> GetJoinedGroup()
        {
            return _groupManager.GetJoinedGroup(whoami());
        }

        /// <summary>
        /// Leave a group
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LeaveGroup(string groupID)
        {
            _groupManager.LeaveGroup(whoami(), groupID);
            return new ActionResult();
        }

        /// <summary>
        /// get all groups that I owned
        /// 
        /// example output:
        /// [
        ///     {
        ///         "GroupID": "msgorilladev",
        ///         "DisplayName": "MSGorilla Dev",
        ///         "Description": "MSgorilla Devs and Testers",
        ///         "IsOpen": false,
        ///         "MemberID": "user1",
        ///         "Role": "admin"
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMembership> GetOwnedGroup()
        {
            return _groupManager.GetOwnedGroup(whoami());
        }

        /// <summary>
        /// Set default group for the current user.
        /// Robot cannot change default group
        /// 
        /// example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
        /// </summary>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public ActionResult SetDefaultGroup(string group)
        {
            string me = whoami();
            MembershipHelper.SetDefaultGroup(me, group);
            return new ActionResult();
        }

        /// <summary>
        /// Get the default group for the current user.
        /// 
        /// example output:
        /// "microsoft"
        /// </summary>
        /// <returns></returns>
        public string GetDefaultGroup()
        {
            return MembershipHelper.DefaultGroup(whoami());
        }

        /// <summary>
        /// Admin create a robot account for his group.
        /// 
        /// example output:
        /// {
        ///     "Userid": "Testrobot",
        ///     "DisplayName": "Robot",
        ///     "PortraitUrl": null,
        ///     "Description": "robot for demo",
        ///     "FollowingsCount": 0,
        ///     "FollowersCount": 0,
        ///     "MessageCount": 0,
        ///     "IsRobot": true
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <param name="userid">robot user id</param>
        /// <param name="password">robot password</param>
        /// <param name="displayname">robot display name</param>
        /// <param name="description">robot description</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayUserProfile CreateGroupRobotAccount(string groupID, string userid, string password, string displayname = null, string description = null)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);

            UserProfile robot = new UserProfile();
            robot.Userid = userid;
            robot.DisplayName = displayname;
            robot.Description = description;
            robot.FollowingsCount = 0;
            robot.FollowersCount = 0;
            robot.MessageCount = 0;
            robot.IsRobot = true;
            robot.Password = Utils.MD5Encoding(password);
            robot.DefaultGroup = groupID;

            return new DisplayUserProfile(_groupManager.CreateGroupRobotAccount(groupID, me, robot), 0);
        }
    }
}