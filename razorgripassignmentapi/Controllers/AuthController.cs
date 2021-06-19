using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;
using razorgripassignmentapi.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OurShouts.API.Controllers
{


    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public AuthController(IOptions<AppSettings> appSettings, IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            Mapper = mapper;
            UserManager = userManager;
            SignInManager = signInManager;
            AppSettings = appSettings.Value;
        }

        public IMapper Mapper { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }
        public AppSettings AppSettings { get; }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLogin)
        {
            ApplicationUser user = null;
            if (userForLogin.UserNameOrEmail.Contains("@"))
                user = await UserManager.FindByEmailAsync(userForLogin.UserNameOrEmail);
                
            if (user == null)
                user = await UserManager.FindByNameAsync(userForLogin.UserNameOrEmail);

            if (user == null)
                return BadRequest("Invalid Credentials");

            var signInResult = await SignInManager.CheckPasswordSignInAsync(user, userForLogin.Password, false);


            if (signInResult.Succeeded)
            {
                var userForReturn = Mapper.Map<UserToReturnDTO>(user);

                return Ok(new { token = GenerateToken(user, userForLogin.RememberMe), user = userForReturn });
            }
            return BadRequest("Invalid Credentials");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTO userForRegister)
        {

            var user = Mapper.Map<ApplicationUser>(userForRegister);

            var registrationResult = await UserManager.CreateAsync(user, userForRegister.Password);

            
            if (registrationResult.Succeeded)
            {
                var _userFromVault = await UserManager.FindByNameAsync(userForRegister.UserName);
                await UserManager.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, _userFromVault.Id));
                await UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, _userFromVault.UserName));
                var userToReturn = Mapper.Map<UserToReturnDTO>(_userFromVault);

                return CreatedAtAction("GetUser", "Users", new { id = user.Id }, userToReturn);
            }
            return BadRequest(registrationResult.Errors);
        }

        [HttpGet("isusernameunique/{username}")]
        public async Task<IActionResult> IsUserNameUnique(string username)
        {
            try
            {
                var isUserPresent = await UserManager.Users.AnyAsync(i => i.UserName == username);

                return Ok(!isUserPresent);
            }
            catch(Exception ex)
            {
                return Ok(false);
            }
           
        }

        string GenerateToken(ApplicationUser user, bool rememberMe = false)
        {
            var claims = new List<Claim>
           {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName),
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Settings.Secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            DateTime expiryDate;
            if (rememberMe)
            {
                expiryDate = DateTime.Now.AddYears(50);
            }
            else
            {
                expiryDate = DateTime.Now.AddDays(1);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryDate,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


    }
}