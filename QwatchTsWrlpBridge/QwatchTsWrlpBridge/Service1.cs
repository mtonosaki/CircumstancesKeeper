// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace QwatchTsWrlpBridge
{
    public partial class Service1 : ServiceBase
    {
        public int ResolutionMilliSeconds { get; set; } = 1000; // Picture porling timer [ms]

        private readonly CancellationTokenSource CancelHandler = new CancellationTokenSource();
        private static readonly MySecretParameter SEC = new MySecretParameter();
        private static CloudBlobClient BlobClient = null;
        private Queue<CircumstancesKeeperModel> UploadQueue = new Queue<CircumstancesKeeperModel>();

        /// <summary>
        /// The constructor
        /// </summary>
        public Service1()
        {
            InitializeComponent();

            var storageAccount = CloudStorageAccount.Parse(SEC.AzureStorageConnectionString);
            BlobClient = storageAccount.CreateCloudBlobClient();
        }

        private string MakeKey(DateTime utcnow)
        {
            var ticks = utcnow.Ticks;
            var rticks = (long)ResolutionMilliSeconds * 1000 * 10;    // unit : 100nano seconds
            ticks /= rticks;
            ticks *= rticks;    // to round down rtick
            var now = new DateTime(ticks);
            var ret = now.ToString("yyyyMMdd-HHmmss-fff");
            return ret;
        }

        /// <summary>
        /// Upload to Azure
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private void UploadProc(object prm)
        {
            var cancellationToken = (CancellationToken)prm;
            var item = new CircumstancesKeeperModel();
            var waitms = 1000;

            try
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    Task.Delay(waitms, cancellationToken).Wait();

                    lock (UploadQueue)
                    {
                        if (UploadQueue.Count > 0)
                        {
                            item = UploadQueue.Dequeue();
                        }
                        else
                        {
                            waitms = 1000;
                            continue;
                        }
                    }

                    var blobcontainer = BlobClient.GetContainerReference($"cam-{item.EventTime.ToString("yyyyMMdd")}-{SEC.Location}");
                    blobcontainer.CreateIfNotExists();
                    var fname = $"{item.Key}{item.ImageType}";
                    var file = blobcontainer.GetBlockBlobReference(fname);
                    file.Metadata["EventTime"] = item.EventTime.ToString(TimeUtil.FormatYMDHMSms);

                    if (item.ImageType != ".ERR")
                    {
                        file.Metadata["Length"] = item.ImageData.Length.ToString();
                        file.UploadFromByteArray(item.ImageData, 0, item.ImageData.Length);
                    }
                    else
                    {
                        file.Metadata["Length"] = "0";
                    }
                    waitms = 10;
#if DEBUG
                    eventlog($"{item.EventTime.ToString(TimeUtil.FormatYMDHMSms)} -- uploaded : {fname}");
#endif
                }
            }
            catch (Exception ex)
            {
                eventlog($"{ex.Message}  {ex.StackTrace}");
                CancelHandler.Cancel();
            }
        }

        /// <summary>
        /// Main service porling task
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ServiceProc(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                var now = DateTime.UtcNow;
                try
                {
                    await Task.Delay(ResolutionMilliSeconds, cancellationToken); // upload 10s each to reduce request count to Azure.

                    var http = WebRequest.Create($"{SEC.Url}/snapshot.jpg");
                    http.Method = "GET";
                    http.ContentType = "image/jpeg";
                    http.Credentials = new NetworkCredential(SEC.UserName, SEC.Password);

                    now = DateTime.UtcNow;
                    var res = await http.GetResponseAsync();
                    using (var bin = new BinaryReader(res.GetResponseStream()))
                    {
                        var dat = new CircumstancesKeeperModel
                        {
                            Key = MakeKey(now),
                            EventTime = now,
                            ImageData = bin.ReadBytes((int)res.ContentLength),
                            ImageType = ".jpg",
                        };
                        lock (UploadQueue)
                        {
                            UploadQueue.Enqueue(dat);
                        }
                    }
                }
                catch (WebException ex)
                {
                    var dat = new CircumstancesKeeperModel
                    {
                        Key = MakeKey(now),
                        EventTime = now,
                        ImageData = new byte[] { },
                        ImageType = ".ERR",
                        ErrorMessage = $"{ex.Message}",
                    };
                    lock (UploadQueue)
                    {
                        UploadQueue.Enqueue(dat);
                    }
                }
                catch (Exception ex)
                {
                    eventlog($"{ex.Message} {ex.StackTrace}");
                    CancelHandler.Cancel();
                    break;
                }
            }
        }

        /// <summary>
        /// Event Log
        /// </summary>
        /// <param name="mes"></param>
        private void eventlog(string mes)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(mes);
            }
            else
            {
                eventLog1.WriteEntry(mes);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void OnStartConsole(string[] args)
        {
            OnStart(args);
            try
            {
                for (; ; )
                {
                    eventlog("To stop debugging, press stop button of Visual Studio");
                    Task.Delay(10000, CancelHandler.Token).Wait();
                }
            }
            catch (Exception)
            {
            }
        }

        private Timer UploadTimer = null;

        protected override void OnStart(string[] args)
        {
            args = Environment.GetCommandLineArgs();
            var isParameterError = false;
            if (isParameterError)
            {
                new Timer(prm =>
                {
                    Stop(); // Delay Service Stop when error.  (otherwise, Service Stop --> Start in Event log)
                }, null, 1000, Timeout.Infinite);
            }
            else
            {
                var t1 = ServiceProc(CancelHandler.Token);
                UploadTimer = new Timer(UploadProc, CancelHandler.Token, 500, Timeout.Infinite);
            }
        }

        public void OnStopConsole()
        {
            OnStop();
        }

        protected override void OnStop()
        {
            CancelHandler.Cancel();
            eventlog($"Stopped SyslogAzureMonitorBridge");
        }
    }
}
