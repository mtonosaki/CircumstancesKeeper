using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CircumstancesKeeperWeb.Controllers
{
    public static class Common
    {
        public static CloudBlobClient Blob = null;
        public static readonly Dictionary<string, CloudBlobContainer> Blobs = new Dictionary<string, CloudBlobContainer>();
        public static LinkedList<string> CacheKeys = new LinkedList<string>();
        public static Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();
        public static long ResolutionMilliSeconds = 1000L;


        public static void Prepare(IOptions<MyConfig> config)
        {
            if (Common.Blob == null)
            {
                var storageAccount = CloudStorageAccount.Parse(config.Value.AzureStorageConnectionString);
                Common.Blob = storageAccount.CreateCloudBlobClient();
            }
        }
        public static string MakeMimeString(string id)
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
    }
}
