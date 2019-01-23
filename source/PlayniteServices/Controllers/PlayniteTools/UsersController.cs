using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayniteServices.Models.Playnite;
using LiteDB;

namespace PlayniteServices.Controllers.PlayniteTools
{
    [Route("playnite/users")]
    public class UsersController : Controller
    {
        private static LiteCollection<User> usersColl = Program.Database.GetCollection<User>("PlayniteUsers");

        [HttpPost]
        public IActionResult Create([FromBody]User user)
        {
            if (user == null)
            {
                return BadRequest(new ErrorResponse(new Exception("No user data provided.")));
            }

            var dbUser = usersColl.FindById(user.Id);
            user.LastLaunch = DateTime.Now;
            if (dbUser == null)
            {               
                usersColl.Insert(user);
            }
            else
            {
                usersColl.Update(user);
            }

            return Ok();
        }
    }
}
