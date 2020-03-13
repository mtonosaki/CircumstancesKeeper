// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tono;

namespace CircumstancesKeeperWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrameController : ControllerBase
    {
        private readonly ILogger<FrameController> _logger;

        public FrameController(IOptions<MyConfig> config, ILogger<FrameController> logger)
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

        // GET: api/Frame/location0001
        /// <summary>
        /// Get Camera Image (JPEG)
        /// </summary>
        /// <param name="l">Location code</param>
        /// <param name="dt">image date time (ISO8601)</param>
        /// <returns>Picture that is for ＜img src="(here)"/＞</returns>
        [HttpGet("{l}", Name = "GetFrame")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult> Get(string l, string dt = null)
        {
            try
            {
                var utc = dt == null
                    ? DateTime.UtcNow - TimeSpan.FromSeconds(10)
                    : DateTime.Parse(dt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var tick = utc.Ticks;   // rounddown to resolution
                tick /= Common.ResolutionMilliSeconds * 10000L;
                tick *= Common.ResolutionMilliSeconds * 10000L;
                utc = new DateTime(tick);

                var conname = $"cam-{utc.ToString("yyyyMMdd")}-{l}";
                var fname = $"{utc.ToString("yyyyMMdd")}-{utc.ToString("HHmmss")}-{utc.ToString("fff")}.jpg";
                var key = conname + "/" + fname;

                MemoryStream ms;
                bool cacheret;
                byte[] buf;
                lock (Common.Cache)
                {
                    cacheret = Common.Cache.TryGetValue(key, out buf);
                }
                if (cacheret  == false)
                {
                    var blobcontainer = Common.Blobs.GetValueOrDefault(conname, true, a => Common.Blob.GetContainerReference(conname));
                    var blob = blobcontainer.GetBlobReference(fname);
                    ms = new MemoryStream();    // keep MemoryStream instance for lazy processing in FileStreamResult. (not use using{} )
                    await blob.DownloadToStreamAsync(ms);

                    lock (Common.Cache)
                    {
                        Common.Cache[key] = ms.ToArray();
                        Common.CacheKeys.AddLast(key);
                        if (Common.CacheKeys.Count > 100)
                        {
                            var delkey = Common.CacheKeys.First;
                            Common.CacheKeys.Remove(delkey);
                            Common.Cache.Remove(delkey.Value);
                        }
                    }
                }
                else
                {
                    ms = new MemoryStream(buf);
                    lock (Common.Cache)
                    {
                        var node = Common.CacheKeys.Find(key);
                        if (node != null)
                        {
                            Common.CacheKeys.Remove(node);
                            Common.CacheKeys.AddLast(key); // change list index
                        }
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);
                var ret = new FileStreamResult(ms, Common.MakeMimeString(fname));
                return ret;
            }
            catch (StorageException ex)
            {
                if (ex.HResult == -2146233088)
                {
                    return StatusCode(404);
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception)
            {
                return StatusCode(500); // Internal Server Error
            }
        }

        #region NOT USED
#if false
        // POST: api/Frame
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Frame/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
#endif
        #endregion
    }
}
