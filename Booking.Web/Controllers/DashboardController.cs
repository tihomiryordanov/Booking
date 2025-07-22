using Microsoft.AspNetCore.Mvc;

namespace Booking.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
