using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnyukamHorgolta.DataAccess.Repository.IRepository;
using AnyukamHorgolta.Models;
using AnyukamHorgolta.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnyukamHorgolta.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController (IUnitOfWork unitOfWork, 
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API Calls

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userList = await _unitOfWork.ApplicationUser.GetAllAsync();
            
            foreach (var user in userList)
            {
                user.Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault().ToString();
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            var usersFilteredData = userList.Select(x => new { 
                Id = x.Id, Name = x.Name, Email = x.Email, PhoneNumber = x.PhoneNumber,
                Company = new { Name = x.Company.Name }, Role = x.Role, LockoutEnd = x.LockoutEnd
            });               

            return Json(new { data = usersFilteredData });
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {
            var user = await _unitOfWork.ApplicationUser.GetFirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while Locking / Unlocking" });
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation successful" });
        }

        #endregion
    }
}