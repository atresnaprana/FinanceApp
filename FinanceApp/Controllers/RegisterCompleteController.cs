using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseLineProject.Controllers
{
    public class RegisterCompleteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
