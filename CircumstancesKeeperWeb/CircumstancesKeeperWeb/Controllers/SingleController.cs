// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using CircumstancesKeeperWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

            var model = new SingleViewModel
            {
            };
            return View(model);
        }
    }
}