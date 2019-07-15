using System.IO;
using Logfile.AspNetCore;
using Logfile.Core;
using Logfile.Structured;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
			// TODO Wait for logfile flushed.
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging((context, builder) =>
				{
					// Set up logfile.
					// Use context.Configuration if configuration is required.
					var structuredRouter = new Logfile.Structured.Router<StandardLoglevel>();
					structuredRouter.ReconfigureAsync(new StructuredLoglevelConfigurationBuilder<StandardLoglevel>()
						.UseConsole()
						.UseAppName("My app")
						.UsePath(Path.Combine(".", "logs"))
						.KeepLogfiles(1)
						.RestrictLogfileSize(10 * 1024 * 1024)
						.Build(), default).GetAwaiter().GetResult();

					var logfile = new StandardLogfile();
					logfile.ReconfigureAsync(new LogfileConfigurationBuilder<StandardLoglevel>()
						.AddRouter(structuredRouter)
						.AllowLoglevels(StandardLoglevel.Information, StandardLoglevel.Critical)
						.Build(), default).GetAwaiter().GetResult();

					// Register logfile for direct use.
					builder.Services.AddSingleton<StandardLogfile>(logfile);
					builder.Services.AddSingleton<ILogfileProxy<StandardLoglevel>>(logfile);

					// Register logfile for ILogger mechanism.
					builder.AddLogfile(logfile);
				})
				.UseStartup<Startup>();
	}
}
