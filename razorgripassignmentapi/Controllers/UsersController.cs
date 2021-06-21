using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using razorgripassignmentapi.Data.DbContext;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;

namespace razorgripassignmentapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper,RazorgripDbContext dbContext)
        {
            UserManager = userManager;
            Mapper = mapper;
            DbContext = dbContext;
        }

        public UserManager<ApplicationUser> UserManager { get; }
        public RazorgripDbContext DbContext { get; set; }
        //public UsersRepository UsersRep { get; }
        public IMapper Mapper { get; }

        [AllowAnonymous]
        [HttpGet("getuser/{id}", Name = "GetUser")]
        public async Task<ActionResult<UserToReturnDTO>> GetUser(int id)
        {
            var user = await UserManager.FindByIdAsync(id.ToString());

            var userForReturn = Mapper.Map<UserToReturnDTO>(user);

            return userForReturn;
        }

        [HttpGet("ping")]
        public ActionResult<Message> Ping()
        {
            try
            {
                var message = DbContext.Messages.FirstOrDefault();
                return Ok(message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { msg = ex.Message });
            }
        }
    }
}