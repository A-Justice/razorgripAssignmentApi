using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;

namespace razorgripassignmentapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            UserManager = userManager;
            Mapper = mapper;
        }

        public UserManager<ApplicationUser> UserManager { get; }
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
    }
}