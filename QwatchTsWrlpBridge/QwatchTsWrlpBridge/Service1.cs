// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QwatchTsWrlpBridge
{
    public partial class Service1 : ServiceBase
    {
        private readonly CancellationTokenSource CancelHandler = new CancellationTokenSource();

        /// <summary>
        /// The constructor
        /// </summary>
        public Service1()
        {
            InitializeComponent();
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
                await Task.Delay(1000, cancellationToken); // upload 10s each to reduce request count to Azure.
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

            for (; ; )
            {
                eventlog("To stop debugging, press stop button of Visual Studio");
                Task.Delay(10000).Wait();
            }
        }

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
                var dmy = ServiceProc(CancelHandler.Token);
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
