using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayniteServices.Models.Playnite;

namespace PlayniteServices.Controllers.Playnite
{
    [Route("api/playnite/users")]
    public class UsersController : Controller
    {
        [HttpPost]
        public IActionResult Create([FromBody]User user)
        {
            if (user == null)
            {
                return BadRequest(new GenericResponse(null, "No user data provided."));
            }

            var usersColl = Program.DatabaseCache.GetCollection<User>("PlayniteUsers");
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
