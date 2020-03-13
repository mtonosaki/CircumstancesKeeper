using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Tono;

namespace CircumstancesKeeperWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LastController : ControllerBase
    {
        private readonly ILogger<LastController> _logger;

        public LastController(IOptions<MyConfig> config, ILogger<LastController> logger)
        {
            Common.Prepare(config);
            _logger = logger;
        }

        // GET: api/Frame
        [HttpGet]
        public ActionResult Get()
        {
            return StatusCode(400); // Bad Request
        }

        // GET: api/Last/location0001    https://localhost:44310/api/Last/location0001
        /// <summary>
        /// Get the latest camera image (JPEG)
        /// </summary>
        /// <param name="l">Location code</param>
        /// <returns>Last item DateTime</returns>
        [HttpGet("{l}", Name = "GetLast")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<DateTime>> Get(string l)
        {
            var utc = DateTime.UtcNow + TimeSpan.FromSeconds(60);
            var conname = $"cam-{utc.ToString("yyyyMMdd")}-{l}";
            var token = default(BlobContinuationToken);
            var blobcontainer = Common.Blobs.GetValueOrDefault(conname, true, a => Common.Blob.GetContainerReference(conname));
            for (var i = 0; i < 5; i++)
            {
                var sret = await blobcontainer.ListBlobsSegmentedAsync($"{utc.ToString("yyyyMMdd")}-{utc.ToString("HHmm")}", true, BlobListingDetails.Metadata, null, token, null, null);
                var col =
                    from r in sret.Results
                    let cbb = r as CloudBlockBlob
                    where cbb != null
                    orderby cbb.Name descending
                    select cbb;
                var first = col.FirstOrDefault();
                if (first != default)
                {
                    var dt = DateTime.Parse(first.Metadata["EventTime"]);
                    return dt;
                } else
                {
                    utc -= TimeSpan.FromMinutes(1.0);
                }
            }
            return StatusCode(404);
        }
    }
}