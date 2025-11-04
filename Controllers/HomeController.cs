using Microsoft.AspNetCore.Mvc;

namespace InventarioRopaTipica.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// GET: /
        /// Página principal - Redirige al login o dashboard
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET: /Home/Error
        /// Página de error
        /// </summary>
        [Route("Home/Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}