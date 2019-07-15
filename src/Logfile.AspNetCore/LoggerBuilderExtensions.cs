using System;
using Logfile.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logfile.AspNetCore
{
	public static class LoggerBuilderExtensions
	{
		public static ILoggingBuilder AddLogfile(this ILoggingBuilder builder, StandardLogfile logfile)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			if (logfile == null) throw new ArgumentNullException(nameof(logfile));

			builder.Services.AddSingleton<ILoggerProvider>(new StandardLoggerProvider(logfile));
			return builder;
		}

		public static ILoggingBuilder AddLogfile<TLoglevel>(this ILoggingBuilder builder, ILogfileProxy<TLoglevel> logfile,
			Func<LogLevel, TLoglevel> loglevelConversion)
			where TLoglevel : Enum
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			if (logfile == null) throw new ArgumentNullException(nameof(logfile));
			if (loglevelConversion == null) throw new ArgumentNullException(nameof(loglevelConversion));

			builder.AddProvider(new LoggerProvider<TLoglevel>(logfile, loglevelConversion));
			return builder;
		}
	}
}
