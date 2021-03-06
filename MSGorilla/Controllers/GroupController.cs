﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Controllers
{
    public class GroupController : Controller
    {
        AccountManager _accManager = new AccountManager();
        GroupManager _grpManager = new GroupManager();

        [TokenAuthAttribute]
        public ActionResult Index()
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "allgroups";
            ViewBag.FeedId = "";
            return View();
        }

        [TokenAuthAttribute]
        public ActionResult View(string group)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            var g = _grpManager.GetGroupByID(group);
            if (g == null)
            {
                ViewBag.FeedCategory = "";
                ViewBag.FeedId = "";
                ViewBag.GroupName = "";
            }
            else
            {
                ViewBag.FeedCategory = "viewgroup";
                ViewBag.FeedId = group;
                ViewBag.GroupName = g.DisplayName;
            }

            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Manage(string group)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            var g = _grpManager.GetGroupByID(group);
            if (g == null)
            {
                ViewBag.FeedCategory = "";
                ViewBag.FeedId = "";
                ViewBag.GroupName = "";
            }
            else
            {
                ViewBag.FeedCategory = "managegroup";
                ViewBag.FeedId = group;
                ViewBag.GroupName = g.DisplayName;
            }

            return View();
        }
    }
}
