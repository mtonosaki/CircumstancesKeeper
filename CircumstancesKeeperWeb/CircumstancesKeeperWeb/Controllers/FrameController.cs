// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
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
        private static CloudBlobClient Blob = null;
        private static readonly Dictionary<string, CloudBlobContainer> Blobs = new Dictionary<string, CloudBlobContainer>();
        private static LinkedList<string> CacheKeys = new LinkedList<string>();
        private static Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();

        public FrameController(IOptions<MyConfig> config)
        {
            if( Blob == null)
            {
                var storageAccount = CloudStorageAccount.Parse(config.Value.AzureStorageConnectionString);
                Blob = storageAccount.CreateCloudBlobClient();
            }
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
        [HttpGet("{l}", Name = "Get")]
        [ResponseCache(Duration = 86400)]
        public async Task<ActionResult> Get(string l, string dt = null)
        {
            try
            {
                var ResolutionMilliSeconds = 1000L;
                var utc = dt == null
                    ? DateTime.UtcNow - TimeSpan.FromSeconds(10)
                    : DateTime.Parse(dt, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var tick = utc.Ticks;   // rounddown to resolution
                tick /= ResolutionMilliSeconds * 10000L;
                tick *= ResolutionMilliSeconds * 10000L;
                utc = new DateTime(tick);

                var conname = $"cam-{utc.ToString("yyyyMMdd")}-{l}";
                var fname = $"{utc.ToString("yyyyMMdd")}-{utc.ToString("HHmmss")}-{utc.ToString("fff")}.jpg";
                var key = conname + "/" + fname;

                MemoryStream ms;
                if (Cache.TryGetValue(key, out var buf) == false)
                {
                    var blobcontainer = Blobs.GetValueOrDefault(l, true, a => Blob.GetContainerReference(conname));
                    var blob = blobcontainer.GetBlobReference(fname);
                    ms = new MemoryStream();    // keep MemoryStream instance for lazy processing in FileStreamResult. (not use using{} )
                    await blob.DownloadToStreamAsync(ms);

                    Cache[key] = ms.ToArray();
                    CacheKeys.AddLast(key);
                    if (CacheKeys.Count > 100)
                    {
                        var delkey = CacheKeys.First;
                        CacheKeys.Remove(delkey);
                        Cache.Remove(delkey.Value);
                    }
                }
                else
                {
                    ms = new MemoryStream(buf);
                    var node = CacheKeys.Find(key);
                    if (node != null)
                    {
                        CacheKeys.Remove(node);
                        CacheKeys.AddLast(key); // change list index
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);
                var ret = new FileStreamResult(ms, makeMimeString(fname));
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

        private string makeMimeString(string id)
        {
            var ext = Path.GetExtension(id);
            string mime;
            switch (ext.ToLower())
            {
                case ".gif":
                    mime = "image/gif";
                    break;
                case ".png":
                    mime = "image/png";
                    break;
                default:
                    mime = "image/jpeg";
                    break;
            }
            return mime;
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
