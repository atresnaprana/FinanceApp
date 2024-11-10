using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using BaseLineProject.Data;
using BaseLineProject.Models;

namespace BaseLineProject.Controllers
{
    public class CreateRoleController : Controller
    {
        RoleManager<IdentityRole> roleManager;
        UserManager<IdentityUser> userManager;
        private readonly FormDBContext db;
        public CreateRoleController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, FormDBContext db)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.db = db;

        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
            var roles = roleManager.Roles.ToList();
            return View(roles);
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            return View(new IdentityRole());
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            await roleManager.CreateAsync(role);
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Edit(string id)
        {
            var rolefld = roleManager.Roles.Where(y => y.Id == id).FirstOrDefault();
            return View(rolefld);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(IdentityRole pRole)
        {
            //await roleManager.UpdateAsync(role);
            try
            {
                var role = await roleManager.FindByIdAsync(pRole.Id);
                role.Name = pRole.Name;
                role.NormalizedName = pRole.NormalizedName;
                var result = await roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First().ToString());
                    return View();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Delete(string id)
        {
            var rolefld = roleManager.Roles.Where(y => y.Id == id).FirstOrDefault();
            return View(rolefld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await roleManager.FindByIdAsync(id);
                var result = await roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First().ToString());
                    return View();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string id)
        {
            ViewBag.roleId = id;

            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            //var testData = userManager.Users.Where(y => )
            var userRole = await userManager.GetUsersInRoleAsync(role.Name);
            var addedrolemodel = new List<UserRoleViewModel>();
            foreach (var users in userRole)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = users.Id,
                    UserName = users.UserName
                };
                addedrolemodel.Add(userRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            foreach (var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                var validate = addedrolemodel.Where(y => y.UserId == user.Id).Count();
                if (validate > 0)
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                //if (await userManager.IsInRoleAsync(user, role.Name))
                //{
                //    userRoleViewModel.IsSelected = true;
                //}
                //else
                //{
                //    userRoleViewModel.IsSelected = false;
                //}

                model.Add(userRoleViewModel);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveMultiData(string formTrans, string roleid)
        {
            var model = JsonConvert.DeserializeObject<List<UserRoleViewModel>>(formTrans);
            //foreach (var test in o)
            //{
            //    var fld = test;
            //    string tests = "";

            //}
            string message = "";
            var role = await roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleid} cannot be found";
                return View("NotFound");
            }
            var userRole = await userManager.GetUsersInRoleAsync(role.Name);
            var addedrolemodel = new List<UserRoleViewModel>();
            foreach (var users in userRole)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = users.Id,
                    UserName = users.UserName
                };
                addedrolemodel.Add(userRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;
                var flds = addedrolemodel.Where(y => y.UserId == model[i].UserId).FirstOrDefault();

                if (model[i].IsSelected && flds == null)
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && flds != null)
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    message = "ok";

                }
                else
                {
                    message = "Error: " + result.Errors;


                }
            }

            //do something
            return Json(message);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditRolesTab(string id)
        {
            ViewBag.roleId = id;

            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new List<SystemTabModel>();
            //var testData = userManager.Users.Where(y => )
            var tabRole = db.TabTbl.Where(y => y.ROLE_ID == role.Name && y.FLAG_AKTIF == "1");
            var addedrolemodel = new List<SystemTabModel>();
            foreach (var tabs in tabRole)
            {
                var TabRoleViewModel = new SystemTabModel
                {
                    ID = tabs.ID,
                    TAB_DESC = tabs.TAB_DESC,
                    TAB_TXT = tabs.TAB_TXT,
                    ROLE_ID = tabs.ROLE_ID
                };
                addedrolemodel.Add(TabRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            var tabAll = db.TabTbl.Where(y => y.FLAG_AKTIF == "1").ToList();
            foreach (var tabs in tabAll)
            {
                var TabRoleViewModel = new SystemTabModel
                {
                    ID = tabs.ID,
                    TAB_DESC = tabs.TAB_DESC,
                    ROLE_ID = tabs.ROLE_ID,
                    TAB_TXT = tabs.TAB_TXT
                };
                var validate = addedrolemodel.Where(y => y.ID == tabs.ID).Count();
                if (validate > 0)
                {
                    TabRoleViewModel.IsSelected = true;
                }
                else
                {
                    TabRoleViewModel.IsSelected = false;
                }
                //if (await userManager.IsInRoleAsync(user, role.Name))
                //{
                //    userRoleViewModel.IsSelected = true;
                //}
                //else
                //{
                //    userRoleViewModel.IsSelected = false;
                //}

                model.Add(TabRoleViewModel);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveMultiDataTab(string formTrans, string roleid)
        {
            var model = JsonConvert.DeserializeObject<List<SystemTabModel>>(formTrans);
            //foreach (var test in o)
            //{
            //    var fld = test;
            //    string tests = "";

            //}
            string message = "ok";
            var role = await roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleid} cannot be found";
                return View("NotFound");
            }

            //var userRole = await userManager.GetUsersInRoleAsync(role.Name);
            var tabRole = db.TabTbl.Where(y => y.ROLE_ID == role.Name && y.FLAG_AKTIF == "1");
            var addedrolemodel = new List<SystemTabModel>();

            foreach (var Tabs in tabRole)
            {
                var TabRoleViewModel = new SystemTabModel
                {
                    ID = Tabs.ID,
                    TAB_DESC = Tabs.TAB_DESC
                };
                addedrolemodel.Add(TabRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            try
            {
                for (int i = 0; i < model.Count; i++)
                {
                    //var user = await userManager.FindByIdAsync(model[i].ID);
                    var TabFld = db.TabTbl.Find(model[i].ID);
                    var MenuList = db.MenuTbl.Where(y => y.TAB_ID == model[i].ID).ToList();
                    var flds = addedrolemodel.Where(y => y.ID == model[i].ID).FirstOrDefault();

                    if (model[i].IsSelected && flds == null)
                    {
                        //result = await userManager.AddToRoleAsync(user, role.Name);
                        TabFld.ROLE_ID = role.Name;
                        foreach (var menufld in MenuList)
                        {
                            var menudb = db.MenuTbl.Find(menufld.ID);
                            if (string.IsNullOrEmpty(menudb.ROLE_ID))
                            {
                                menudb.ROLE_ID = role.Name;
                            }
                            db.SaveChanges();
                        }

                    }
                    else if (!model[i].IsSelected && flds != null)
                    {
                        //result = await userManager.RemoveFromRoleAsync(user, role.Name);
                        TabFld.ROLE_ID = "";
                        foreach (var menufld in MenuList)
                        {
                            var menudb = db.MenuTbl.Find(menufld.ID);
                            if (menudb.ROLE_ID == role.Name)
                            {
                                menufld.ROLE_ID = "";
                            }
                            db.SaveChanges();
                        }
                    }
                    else if (model[i].IsSelected && flds != null)
                    {
                        //result = await userManager.AddToRoleAsync(user, role.Name);
                        foreach (var menufld in MenuList)
                        {
                            var menudb = db.MenuTbl.Find(menufld.ID);
                            if (string.IsNullOrEmpty(menudb.ROLE_ID))
                            {
                                menudb.ROLE_ID = role.Name;
                            }
                            db.SaveChanges();
                        }

                    }
                    else
                    {
                        continue;
                    }


                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }


            //do something
            return Json(message);
        }
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> EditRolesMenu(string id)
        {
            ViewBag.roleId = id;

            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new List<SystemMenuModel>();
            //var testData = userManager.Users.Where(y => )
            var menuRole = db.MenuTbl.Where(y => y.ROLE_ID == role.Name && y.FLAG_AKTIF == "1");
            var addedrolemodel = new List<SystemMenuModel>();
            foreach (var menus in menuRole)
            {
                var MenuRoleViewModel = new SystemMenuModel
                {
                    ID = menus.ID,
                    MENU_DESC = menus.MENU_DESC,
                    MENU_TXT = menus.MENU_TXT,
                    ROLE_ID = menus.ROLE_ID
                };
                addedrolemodel.Add(MenuRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            var menuAll = db.MenuTbl.Where(y => y.FLAG_AKTIF == "1").ToList();
            foreach (var menus in menuAll)
            {
                var MenuRoleViewModel = new SystemMenuModel
                {
                    ID = menus.ID,
                    MENU_DESC = menus.MENU_DESC,
                    MENU_TXT = menus.MENU_TXT,
                    ROLE_ID = menus.ROLE_ID
                };
                var validate = addedrolemodel.Where(y => y.ID == menus.ID).Count();
                if (validate > 0)
                {
                    MenuRoleViewModel.IsSelected = true;
                }
                else
                {
                    MenuRoleViewModel.IsSelected = false;
                }
                //if (await userManager.IsInRoleAsync(user, role.Name))
                //{
                //    userRoleViewModel.IsSelected = true;
                //}
                //else
                //{
                //    userRoleViewModel.IsSelected = false;
                //}

                model.Add(MenuRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMultiDataMenu(string formTrans, string roleid)
        {
            var model = JsonConvert.DeserializeObject<List<SystemMenuModel>>(formTrans);
            //foreach (var test in o)
            //{
            //    var fld = test;
            //    string tests = "";

            //}
            string message = "ok";
            var role = await roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleid} cannot be found";
                return View("NotFound");
            }

            //var userRole = await userManager.GetUsersInRoleAsync(role.Name);
            var menuRole = db.MenuTbl.Where(y => y.ROLE_ID == role.Name && y.FLAG_AKTIF == "1");
            var addedrolemodel = new List<SystemMenuModel>();

            foreach (var menus in menuRole)
            {
                var TabRoleViewModel = new SystemMenuModel
                {
                    ID = menus.ID,
                    MENU_DESC = menus.MENU_DESC
                };
                addedrolemodel.Add(TabRoleViewModel);
                //model.Users.Add(user.UserName);
            }
            try
            {
                for (int i = 0; i < model.Count; i++)
                {
                    //var user = await userManager.FindByIdAsync(model[i].ID);
                    var MenuFld = db.MenuTbl.Find(model[i].ID);
                    var flds = addedrolemodel.Where(y => y.ID == model[i].ID).FirstOrDefault();

                    if (model[i].IsSelected && flds == null)
                    {
                        //result = await userManager.AddToRoleAsync(user, role.Name);
                        MenuFld.ROLE_ID = role.Name;
                    }
                    else if (!model[i].IsSelected && flds != null)
                    {
                        //result = await userManager.RemoveFromRoleAsync(user, role.Name);
                        MenuFld.ROLE_ID = "";

                    }
                    else
                    {
                        continue;
                    }


                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }


            //do something
            return Json(message);
        }
    }
}
