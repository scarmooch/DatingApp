using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        public AdminController(DataContext context, UserManager<User> userManager)
        {
            this._userManager = userManager;
            this._context = context;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("userswithroles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await _context.Users
                .OrderBy(x => x.UserName)
                .Select(user => new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (from userRole in user.UserRoles
                             join role in _context.Roles
                             on userRole.RoleId
                             equals role.Id
                             select role.Name).ToList()
                }).ToListAsync();

            // code above is linq (for Roles clause). W/o linq same as:
            // var usersWithRoles = await _context.Users
            //     .OrderBy(u => u.UserName)
            //     .Select(u => new 
            //     {
            //         u.Id,
            //         u.UserName,
            //         Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            //     }).ToListAsync();                

            //return Ok("Only admins can see this");
            return Ok(userList);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);
            // roles currenty applied
            var userRoles = await _userManager.GetRolesAsync(user);
            // new roles
            var selectedRoles = roleEditDto.RoleNames;

            // need to cover the fact that a user can be removed from all roles

            // ?? null coalescing operator. same as:
            // selectedRoles = selectedRoles != null? selectedRoles : new string[] {};
            selectedRoles = selectedRoles ?? new string[] {};
            // add roles not already in the string list
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if(!result.Succeeded)
                return BadRequest("Failed to add to roles");
                // remove roles that are not specified in the new roles list
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if(!result.Succeeded)
                return BadRequest("Failed to remove from roles");
            // return roles now applied after changes
            return Ok(await _userManager.GetRolesAsync(user));

        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}