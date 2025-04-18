using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    public class APIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
