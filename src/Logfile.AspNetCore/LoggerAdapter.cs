using System;
using System.Linq;
using Logfile.Core;
using Logfile.Core.Details;
using Logfile.Core.Preprocessors;
using Microsoft.Extensions.Logging;

namespace Logfile.AspNetCore
{
	/// <summary>
	/// Implements an adapter to use <c>Logfile</c> as an ASP.NET Core Logger.
	/// </summary>
	public class LoggerAdapter<TLoglevel> : ILogger
		where TLoglevel : Enum
	{
		#region --- Constants ---

		public const string AspNetCoreEventIDText = "AspNetCore";
		public const int AspNetCoreEventID = -10;

		#endregion

		#region --- Fields ---

		Func<LogLevel, TLoglevel> loglevelConversionCallback;

		#endregion

		#region --- Properties ---

		/// <summary>
		/// Gets the logfile.
		/// </summary>
		public ILogfileProxy<TLoglevel> Logfile { get; }

		/// <summary>
		/// Gets the category name.
		/// </summary>
		public string CategoryName { get; }

		#endregion

		#region --- Constructors ---

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggerAdapter{TLoglevel}"/> class.
		/// </summary>
		/// <param name="logfile">The logfile.</param>
		/// <param name="categoryName">The category name.</param>
		/// <param name="loglevelConversion">The callback method to convert an ASP.NET Core
		///		loglevel into a Logfile loglevel.</param>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logfile"/> is null.</exception>
		public LoggerAdapter(ILogfileProxy<TLoglevel> logfile, string categoryName,
			Func<LogLevel, TLoglevel> loglevelConversion)
		{
			this.Logfile = logfile ?? throw new ArgumentNullException(nameof(logfile));
			CategoryName = categoryName;
			this.loglevelConversionCallback = loglevelConversion ?? throw new ArgumentNullException(nameof(loglevelConversion));
		}

		#endregion

		#region --- ILogger implementation ---

		public IDisposable BeginScope<TState>(TState state)
		{
			// TODO Building blocks is currently not supported.
			return DummyDisposable.Instance;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			if (logLevel == LogLevel.None) return false;

			if (this.Logfile is Logfile<TLoglevel> main)
			{
				TLoglevel logfileLoglevel;

				try
				{
					// Map loglevel first.
					logfileLoglevel = this.loglevelConversionCallback(logLevel);
				}
				catch
				{
					// Error while converting the loglevel.
					return false;
				}

				var loglevelPreprocessors = main.Configuration.Preprocessors.OfType<LoglevelFilter<TLoglevel>>();
				foreach (var preprocessor in loglevelPreprocessors)
				{
					// If blocked the loglevel is not considered enabled.
					if (preprocessor.BlockLoglevels.Contains(logfileLoglevel)) return false;
					// If allowed loglevels exist but the loglevel is not below it is not considered enabled.
					if ((preprocessor.AllowLoglevels.Any()) && (!preprocessor.AllowLoglevels.Contains(logfileLoglevel))) return false;
				}

				// All preprocessors passed.
				return true;
			}
			else
			{
				// If logfile configuration is not accessible, have the implementation
				// evaluate whether the loglevel is enabled or not.
				return true;
			}
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			try
			{
				// Map loglevel first.
				var logfileLoglevel = this.loglevelConversionCallback(logLevel);

				// Write to logfile. Create new log event from converted loglevel first.
				var logEvent = this.Logfile.New(logfileLoglevel);

				// Keep event ID if not the default one.
				if (eventId != default)
				{
					var textChain = new[] { AspNetCoreEventIDText }.AsEnumerable();
					if (!string.IsNullOrWhiteSpace(this.CategoryName))
						textChain = textChain.Append(this.CategoryName);
					if (!string.IsNullOrWhiteSpace(eventId.Name))
						textChain = textChain.Append(eventId.Name);
					logEvent.Details.Add(new Logfile.Core.Details.EventID(
						textChain,
						new[] { AspNetCoreEventID, eventId.Id }));
				}

				// Have state (and exception, in case both can together generate specific output) being formatted.
				if (state != null)
					logEvent.Msg(formatter(state, exception));

				// Treat exception as specific detail even in case it has already been formatted along with a state.
				if (exception != null)
					logEvent.Exception(exception);

				logEvent.Details.Add(new Core.Details.LogfileHierarchy(this.Logfile.Hierarchy));

				logEvent.Log();
			}
			catch
			{
				// Silently fail in case of any errors, because it's just logging.
				return;
			}
		}

		#endregion
	}

	/// <summary>
	/// Implements an adapter to use <c>Logfile</c> with standard loglevels
	/// as an ASP.NET Core Logger.
	/// </summary>
	public class StandardLoggerAdapter : LoggerAdapter<StandardLoglevel>
	{
		/// <summary>
		/// Gets the logfile.
		/// </summary>
		new public StandardLogfile Logfile { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardLoggerAdapter"/> class.
		/// </summary>
		/// <param name="logfile">The logfile.</param>
		/// <param name="categoryName">The category name.</param>
		/// <param name="loglevelConversion">The callback method to convert an ASP.NET Core
		///		loglevel into a Logfile loglevel.</param>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logfile"/> is null.</exception>
		public StandardLoggerAdapter(StandardLogfile logfile, string categoryName)
			: base(logfile, categoryName, StandardLoggerProvider.StandardLoglevelConversion)
		{
			this.Logfile = logfile;
		}
	}

	/// <summary>
	/// Implements an adapter to use <c>Logfile</c> as an ASP.NET Core Logger
	/// with a specific category name.
	/// </summary>
	public class LoggerAdapter<TLoglevel, TCategoryName> : LoggerAdapter<TLoglevel>, ILogger<TCategoryName>
		where TLoglevel : Enum
	{
		#region --- Constructors ---

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggerAdapter{TLoglevel, TCategoryName}"/> class.
		/// </summary>
		/// <param name="logfile">The logfile.</param>
		/// <param name="categoryName">The category name.</param>
		/// <param name="loglevelConversion">The callback method to convert an ASP.NET Core
		///		loglevel into a Logfile loglevel.</param>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logfile"/> is null.</exception>
		public LoggerAdapter(ILogfileProxy<TLoglevel> logfile, string categoryName,
			Func<LogLevel, TLoglevel> loglevelConversion)
			: base(logfile, categoryName, loglevelConversion)
		{

		}

		#endregion
	}

	/// <summary>
	/// Implements an adapter to use <c>Logfile</c> with standard loglevels
	/// as an ASP.NET Core Logger with a specific category name.
	/// </summary>
	public class StandardLoggerAdapter<TCategoryName> : StandardLoggerAdapter, ILogger<TCategoryName>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StandardLoggerAdapter{TLoglevel, TCategoryName}"/> class.
		/// </summary>
		/// <param name="logfile">The logfile.</param>
		/// <param name="categoryName">The category name.</param>
		/// <param name="loglevelConversion">The callback method to convert an ASP.NET Core
		///		loglevel into a Logfile loglevel.</param>
		/// <exception cref="ArgumentNullException">Thrown if
		///		<paramref name="logfile"/> is null.</exception>
		public StandardLoggerAdapter(StandardLogfile logfile, string categoryName)
			: base(logfile, categoryName)
		{
		}
	}

	public class DummyDisposable : IDisposable
	{
		public static readonly DummyDisposable Instance = new DummyDisposable();
		public void Dispose()
		{
		}
	}
}
