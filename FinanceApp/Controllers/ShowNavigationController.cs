using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BaseLineProject.Data;
using BaseLineProject.Models;

namespace BaseLineProject.Controllers
{
    public class ShowNavigationController : Controller
    {
        private readonly FormDBContext db;
        UserManager<IdentityUser> userManager;

        public ShowNavigationController(FormDBContext context, UserManager<IdentityUser> userManager)
        {
            db = context;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            List<SystemTabModel> tabList = new List<SystemTabModel>();
            List<SystemMenuModel> MenuList = new List<SystemMenuModel>();
            if (User.Identity.IsAuthenticated)
            {
                //var roles = User.Claims.ToList();
                var roles = User.FindAll(ClaimTypes.Role).ToList(); // will give the user's userName

                List<string> roleList = new List<string>();
                foreach (var fld in roles)
                {
                    roleList.Add(fld.Value);
                }
                var TabDb = db.TabTbl.Where(y => roleList.Contains(y.ROLE_ID) && y.FLAG_AKTIF == "1").ToList();
                var MenuDB = db.MenuTbl.Where(y => roleList.Contains(y.ROLE_ID) && y.FLAG_AKTIF == "1").ToList();
                tabList = TabDb.Select(y => new SystemTabModel()
                {
                    ID = y.ID,
                    TAB_TXT = y.TAB_TXT,
                    menuList = MenuDB.Where(x => x.TAB_ID == y.ID).ToList()
                }).ToList();
            }
            return PartialView(tabList);
        }
    }
}
