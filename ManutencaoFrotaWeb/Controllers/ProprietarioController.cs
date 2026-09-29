using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManutencaoFrotaWeb.Controllers
{
    public class ProprietarioController : Controller
    {
        //[Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
