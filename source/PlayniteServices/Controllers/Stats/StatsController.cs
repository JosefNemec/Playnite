﻿using LiteDB;
using Microsoft.AspNetCore.Mvc;
using PlayniteServices.Models.Playnite;
using PlayniteServices.Models.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Stats
{    
    public class StatsController : Controller
    {
        private static LiteCollection<User> usersColl = Program.Database.GetCollection<User>("PlayniteUsers");

        [HttpGet("api/stats/{serviceKey}")]
        public GenericResponse GetStarts(string serviceKey)
        {
            var key = Startup.Configuration.GetSection("ServiceKey");
            if (key == null || key.Value != serviceKey)
            {
                return new ErrorResponse(new Exception("Invalid service key."));
            }

            var stats = new ServiceStats();
            var users = usersColl.FindAll().ToList();
            stats.UserCount = users.Count;

            var now = DateTime.Now;
            var lastWeekUsers = users.Where(a => (now - a.LastLaunch).Days <= 7).ToList();
            stats.LastWeekUserCount = lastWeekUsers.Count();

            var userGroups = lastWeekUsers.GroupBy(ver => ver.PlayniteVersion, user => user);
            stats.UsersByVersion = new Dictionary<string, int>();
            foreach (var userGroup in userGroups.OrderByDescending(a => a.Key))
            {
                stats.UsersByVersion.Add(userGroup.Key, userGroup.Count());
            }

            var winGroups = lastWeekUsers.GroupBy(ver => ver.WinVersion, user => user);
            stats.UsersByWinVersion = new Dictionary<string, int>();
            foreach (var winGroup in winGroups.OrderByDescending(a => a.Key))
            {
                stats.UsersByWinVersion.Add(winGroup.Key, winGroup.Count());
            }

            stats.RecentUsers = lastWeekUsers.OrderBy(a => a.LastLaunch).TakeLast(20).ToList();
            stats.X64Count = lastWeekUsers.Where(a => a.Is64Bit).Count();
            stats.X86Count = lastWeekUsers.Where(a => !a.Is64Bit).Count();
            return new ServicesResponse<ServiceStats>(stats);
        }

        [HttpGet("api/stats/drop/{serviceKey}")]
        public GenericResponse DropStats(string serviceKey)
        {
            var key = Startup.Configuration.GetSection("ServiceKey");
            if (key == null || key.Value != serviceKey)
            {
                return new ErrorResponse(new Exception("Invalid service key."));
            }            

            return new ServicesResponse<bool>(Program.Database.DropCollection("PlayniteUsers"));
        }
    }
}
