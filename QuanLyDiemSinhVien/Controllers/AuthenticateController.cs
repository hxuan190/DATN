using asd123.Biz.Roles;
using asd123.Helpers;
using asd123.Model;
using asd123.Presenters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace asd123.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }
        [Authorize]
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            // Implement any server-side logout logic here if needed, such as invalidating tokens
            // For example, you might add the token to a blacklist

            return Ok(new Response { Status = "Success", Message = "Logged out successfully!" });
        }

        [HttpPost]
        [Route("register")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            // Check if any users exist
            var usersCount = await userManager.Users.CountAsync();
            if (usersCount == 1)
            {
                // If no users exist, assign the admin role to the first user
                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            else
            {
                // Assign the user role to subsequent users
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        
        

        // [HttpPost]
        // [Route("register")]
        // public async Task<IActionResult> Register([FromBody] RegisterModel model)
        // {
        //     var userExists = await userManager.FindByNameAsync(model.Username);
        //     if (userExists != null)
        //         return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
        //
        //     ApplicationUser user = new ApplicationUser()
        //     {
        //         Email = model.Email,
        //         SecurityStamp = Guid.NewGuid().ToString(),
        //         UserName = model.Username
        //     };
        //     var result = await userManager.CreateAsync(user, model.Password);
        //     if (!result.Succeeded)
        //         return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
        //     if (!await roleManager.RoleExistsAsync(UserRoles.User))
        //         await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        //     return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        // }

        // [HttpPost]
        // [Route("register-admin")]
        // public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        // {
        //     var userExists = await userManager.FindByNameAsync(model.Username);
        //     if (userExists != null)
        //         return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });
        //
        //     ApplicationUser user = new ApplicationUser()
        //     {
        //         Email = model.Email,
        //         SecurityStamp = Guid.NewGuid().ToString(),
        //         UserName = model.Username
        //     };
        //     var result = await userManager.CreateAsync(user, model.Password);
        //     if (!result.Succeeded)
        //         return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
        //
        //     if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
        //         await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        //     if (!await roleManager.RoleExistsAsync(UserRoles.User))
        //         await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        //
        //     if (await roleManager.RoleExistsAsync(UserRoles.Admin))
        //     {
        //         await userManager.AddToRoleAsync(user, UserRoles.Admin);
        //     }
        //
        //     return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        // }
    }
}