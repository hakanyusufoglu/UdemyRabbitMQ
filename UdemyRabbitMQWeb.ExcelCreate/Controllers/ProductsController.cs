using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
