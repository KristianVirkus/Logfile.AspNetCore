using System;
using System.Threading;
using System.Threading.Tasks;
using Logfile.AspNetCore;
using Logfile.Core;
using Logfile.Core.Details;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication
{
	public class TestService : IHostedService
	{
		ILogger<TestService> logger;
		CancellationTokenSource cts;

		public TestService(ILogger<TestService> logger)
		{
			this.logger = logger;
			this.cts = new CancellationTokenSource();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			Task.Run(async () =>
			{
				var i = 0;
				while (!this.cts.IsCancellationRequested)
				{
					this.logger.Log(LogLevel.Information, $"Logged {++i} time(s) via ILogger.");
					await Task.Delay(TimeSpan.FromSeconds(1), this.cts.Token);
				}
			});

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			this.cts.Cancel();
			return Task.CompletedTask;
		}
	}
}
