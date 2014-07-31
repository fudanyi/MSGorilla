﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;

namespace MSGorilla.Library
{
    public class GroupManager
    {
        public Group GetGroupByID(string GroupID, MSGorillaEntities _gorillaCtx = null)
        {
            if (_gorillaCtx == null)
            {
                using (_gorillaCtx = new MSGorillaEntities())
                {
                    return _gorillaCtx.Groups.Find(GroupID);
                }
            }
            return _gorillaCtx.Groups.Find(GroupID);
        }

        public List<Group> GetAllGroup()
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Groups.ToList();
            }
        }

        public Group AddGroup(string creater, string groupID, string displayName, string description, bool isOpen)
        {
            if (!Utils.IsValidID(groupID))
            {
                throw new InvalidIDException();
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = GetGroupByID(groupID);
                if (group != null)
                {
                    throw new GroupAlreadyExistException(groupID);
                }

                //add group
                group = new Group();
                group.GroupID = groupID;
                group.DisplayName = displayName;
                group.Description = description;
                group.IsOpen = isOpen;
                _gorillaCtx.Groups.Add(group);
                //add creater as default admin
                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = creater;
                member.Role = "admin";
                _gorillaCtx.Memberships.Add(member);

                _gorillaCtx.SaveChanges();
                return group;
            }
        }

        public Group UpdateGroup(string admin, Group group)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group groupUpdate = _gorillaCtx.Groups.Find(group.GroupID);
                if (groupUpdate == null)
                {
                    throw new GroupNotExistException();
                }
                groupUpdate.Description = group.Description;
                groupUpdate.DisplayName = group.DisplayName;
                groupUpdate.IsOpen = group.IsOpen;

                _gorillaCtx.SaveChanges();
                return groupUpdate;
            }
        }

        public Membership JoinGroup(string userid, string groupID)
        {
            using(var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = GetGroupByID(groupID, _gorillaCtx);
                if (group == null)
                {
                    throw new GroupNotExistException();
                }

                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot)
                {
                    throw new HandleRobotMembershipException();
                }

                Membership member = _gorillaCtx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();

                if (member != null)
                {
                    return member;
                }

                if (group.IsOpen == false)
                {
                    throw new UnauthroizedActionException();
                }

                member = new Membership();
                member.GroupID = groupID;
                member.MemberID = userid;
                member.Role = "user";
                _gorillaCtx.Memberships.Add(member);
                _gorillaCtx.SaveChanges();

                return member;
            }
        }

        public Membership CheckMembership(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return CheckMembership(groupID, userid, _gorillaCtx);
            }
        }

        public Membership CheckMembership(string groupID, string userid, MSGorillaEntities _gorillaCtx)
        {
            Group group = _gorillaCtx.Groups.Find(groupID);
            if (group == null)
            {
                throw new GroupNotExistException();
            }

            Membership member = _gorillaCtx.Memberships.SqlQuery(
                "select * from membership where groupid={0} and memberid={1}",
                groupID,
                userid).FirstOrDefault();

            if (member == null)
            {
                throw new UnauthroizedActionException();
            }

            return member;
        }

        public void CheckAdmin(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                CheckAdmin(groupID, userid, _gorillaCtx);
            }
        }

        public void CheckAdmin(string groupID, string userid, MSGorillaEntities ctx)
        {
            if (!"admin".Equals(CheckMembership(groupID, userid, ctx).Role))
            {
                throw new UnauthroizedActionException();
            }
        }

        public Membership AddMember(string groupID, string userid, string role = "user")
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot == true)
                {
                    throw new HandleRobotMembershipException();
                }

                Membership member = _gorillaCtx.Memberships.Where(m => m.GroupID == groupID && m.MemberID == userid).FirstOrDefault();
                if (member == null)
                {
                    member = new Membership();
                    member.GroupID = groupID;
                    member.MemberID = user.Userid;
                    member.Role = role;
                    _gorillaCtx.Memberships.Add(member);

                    _gorillaCtx.SaveChanges();
                }
                
                return member;
            }
        }

        public void RemoveMember(string groupID, string admin, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Membership member = _gorillaCtx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();
                if (member != null)
                {
                    _gorillaCtx.Memberships.Remove(member);
                }
                _gorillaCtx.SaveChanges();
            }
        }

        public Membership UpdateMembership(string groupID, string admin, string userid, string role)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Membership member = CheckMembership(groupID, userid, _gorillaCtx);
                if ("robot".Equals(member.Role))
                {
                    throw new HandleRobotMembershipException();
                }
                member.Role = role;
                _gorillaCtx.SaveChanges();
                return member;
            }
        }

        public List<Membership> GetAllGroupMember(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Memberships.SqlQuery("select * from membership where groupid={0}", groupID).ToList<Membership>();
            }
        }

        public List<DisplayMembership> GetJoinedGroup(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0}",
                    userid
                    ).ToList();
            }
        }

        public List<DisplayMembership> GetOwnedGroup(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0} and role='admin'",
                    userid
                    ).ToList();
            }
        }

        public UserProfile CreateGroupRobotAccount(string groupID, string admin, UserProfile robot)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                robot.IsRobot = true;
                new AccountManager().AddUser(robot);

                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = robot.Userid;
                member.Role = "robot";
                _gorillaCtx.Memberships.Add(member);

                _gorillaCtx.SaveChanges();

                return robot;
            }
        }
    }
}