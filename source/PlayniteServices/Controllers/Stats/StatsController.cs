﻿using Microsoft.AspNetCore.Mvc;
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
        private const string UsersCollection = "PlayniteUsers";

        [HttpGet("api/stats/{serviceKey}")]
        public ServicesResponse<ServiceStats> GetStarts(string serviceKey)
        {
            var key = Startup.Configuration.GetSection("ServiceKey");
            if (key == null || key.Value != serviceKey)
            {
                return new ServicesResponse<ServiceStats>(null, "Invalid service key.");
            }

            var stats = new ServiceStats();
            var users = Program.DatabaseCache.GetCollection<User>(UsersCollection).FindAll().ToList();
            stats.UserCount = users.Count;

            var now = DateTime.Now;
            stats.LastWeekUserCount = users.Where(a => (now - a.LastLaunch).Days <= 7).Count();

            var userGroups = users.GroupBy(ver => ver.PlayniteVersion, user => user);
            stats.UsersByVersion = new Dictionary<string, int>();
            foreach (var userGroup in userGroups.OrderByDescending(a => a.Key))
            {
                stats.UsersByVersion.Add(userGroup.Key, userGroup.Count());
            }

            var winGroups = users.GroupBy(ver => ver.WinVersion, user => user);
            stats.UsersByWinVersion = new Dictionary<string, int>();
            foreach (var winGroup in winGroups.OrderByDescending(a => a.Key))
            {
                stats.UsersByWinVersion.Add(winGroup.Key, winGroup.Count());
            }

            stats.RecentUsers = users.OrderBy(a => a.LastLaunch).TakeLast(20).ToList();
            return new ServicesResponse<ServiceStats>(stats, string.Empty);
        }

        [HttpGet("api/stats/drop/{serviceKey}")]
        public ServicesResponse<bool> DropStats(string serviceKey)
        {
            var key = Startup.Configuration.GetSection("ServiceKey");
            if (key == null || key.Value != serviceKey)
            {
                return new ServicesResponse<bool>(false, "Invalid service key.");
            }            

            return new ServicesResponse<bool>(Program.DatabaseCache.DropCollection(UsersCollection), string.Empty);
        }
    }
}
