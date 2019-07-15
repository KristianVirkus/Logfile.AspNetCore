using System;
using System.Collections.Concurrent;
using System.Linq;
using Logfile.Core;
using Microsoft.Extensions.Logging;

namespace Logfile.AspNetCore
{
	/// <summary>
	/// Implements a provider to create logger instances.
	/// </summary>
	public class LoggerProvider<TLoglevel> : ILoggerProvider
		where TLoglevel : Enum
	{
		#region --- Fields ---

		ConcurrentDictionary<string, ILogger> loggers;
		private readonly Func<LogLevel, TLoglevel> loglevelConversion;

		#endregion

		#region --- Properties ---

		public ILogfileProxy<TLoglevel> Logfile { get; }

		#endregion

		#region --- Constructors ---

		public LoggerProvider(ILogfileProxy<TLoglevel> logfile, Func<LogLevel, TLoglevel> loglevelConversion)
		{
			this.Logfile = logfile ?? throw new ArgumentNullException(nameof(logfile));
			this.loglevelConversion = loglevelConversion ?? throw new ArgumentNullException(nameof(loglevelConversion));
			this.loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.CurrentCulture);
		}

		#endregion

		#region --- ILoggerProvider implementation ---

		public ILogger CreateLogger(string categoryName)
		{
			var logger = this.loggers?.GetOrAdd(categoryName, (name) => new LoggerAdapter<TLoglevel>(this.Logfile, categoryName, this.loglevelConversion));
			if (logger == null) throw new ObjectDisposedException(null);
			return logger;
		}

		public void Dispose()
		{
			var loggersCopy = this.loggers;
			this.loggers = null;
			loggersCopy.Clear();
		}

		#endregion
	}

	public class StandardLoggerProvider : LoggerProvider<StandardLoglevel>
	{
		public StandardLoggerProvider(ILogfileProxy<StandardLoglevel> logfile)
			: base(logfile, StandardLoglevelConversion)
		{
		}

		public static StandardLoglevel StandardLoglevelConversion(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Trace: return StandardLoglevel.Trace;
				case LogLevel.Debug: return StandardLoglevel.Debug;
				case LogLevel.Information: return StandardLoglevel.Information;
				case LogLevel.Warning: return StandardLoglevel.Warning;
				case LogLevel.Error: return StandardLoglevel.Error;
				case LogLevel.Critical: return StandardLoglevel.Critical;
				default: throw new NotSupportedException($"The ASP.NET Core loglevel {logLevel} is not supported.");
			}
		}
	}
}
