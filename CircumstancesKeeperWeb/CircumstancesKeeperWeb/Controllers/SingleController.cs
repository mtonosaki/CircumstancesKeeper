// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CircumstancesKeeperWeb.Controllers
{
    public class SingleController : Controller
    {
        private readonly ILogger<SingleController> _logger;

        public SingleController(ILogger<SingleController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string l)
        {
            ViewBag.Location = l;
            return View();
        }
    }
}