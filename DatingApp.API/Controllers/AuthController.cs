using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        // private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(/*IAuthRepository repo, */IConfiguration config, IMapper mapper,
                            UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
            this._config = config;
            // this._repo = repo;
        }
        [HttpPost("register")]
        // if not user [ApiController] attrib use [FromBody]
        //public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // add this if not using [ApiController] attrib
            // validate the request
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);

            // S20.204 SignIn & User Managers now take care of that
            // userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            // if (await _repo.UserExists(userForRegisterDto.Username))
            // {
            //     return BadRequest("Username already exists");
            // }

            // changed to user automapper S12.131
            // var userToCreate = new User
            // {
            //     Username = userForRegisterDto.Username
            // };
            var userToCreate = _mapper.Map<User>(userForRegisterDto);
            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.Password);

            // S20.204 SignIn & User Managers now take care of that
            // var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            // was just temp, changed S12.131
            // return StatusCode(201);
            // S20.204 now using user manager
            //var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
            //return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);

            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);
            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);
            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);
                return Ok(new
                {
                    token = GenerateJwtToken(user).Result,
                    user = appUser      // this is so that the string is "user", other would have to chage SPA code
                });
            }
            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            //var claims = new[]
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}