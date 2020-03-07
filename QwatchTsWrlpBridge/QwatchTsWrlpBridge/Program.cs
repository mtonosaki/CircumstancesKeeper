// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace QwatchTsWrlpBridge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
			args = Environment.GetCommandLineArgs();   // To get command line args
			var service = new Service1();
			if (Environment.UserInteractive)    // Console Version
			{
				if (args.Length > 0)
				{
					// Support Service Install / Uninstall
					var isServiceExists = ServiceController.GetServices().Any(s => s.ServiceName == service.ServiceName);
					var path = Assembly.GetExecutingAssembly().Location;
					switch (args[0].ToLower())
					{
						case "/i":
							if (isServiceExists)
							{
								Console.WriteLine($"The service named '{service.ServiceName}' have already registered.");
							}
							else
							{
								ManagedInstallerClass.InstallHelper(new[] { path });
							}
							return;
						case "/u":
							if (isServiceExists)
							{
								ManagedInstallerClass.InstallHelper(new[] { "/u", path });
							}
							else
							{
								Console.WriteLine($"Could not install the service '{service.ServiceName}'");
							}
							return;
					}
				}
				service.OnStartConsole(args);
				service.OnStopConsole();

				Console.WriteLine($"\r\n\r\n==== Hit any key to exit from debugging session.");
				Console.Read();
			}
			else
			{    // サービスから起動
				ServiceBase.Run(new[] { service, });
			}

		}
	}
}
