using Microsoft.AspNetCore.Mvc;

namespace My_Endpoint.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}