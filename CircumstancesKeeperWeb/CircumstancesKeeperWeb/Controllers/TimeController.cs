// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using System;

namespace CircumstancesKeeperWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeController : ControllerBase
    {
        // GET: api/Time
        [HttpGet]
        public string Get()
        {
            return DateTime.UtcNow.ToString("o");
        }
    }
}