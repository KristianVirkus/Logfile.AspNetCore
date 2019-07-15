using Logfile.Core;
using Logfile.Core.Details;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LogController : ControllerBase
	{
		static int count = 0;

		readonly ILogfileProxy<StandardLoglevel> logfile;

		public LogController(ILogfileProxy<StandardLoglevel> logfile)
		{
			this.logfile = logfile;
		}

		// GET api/log
		[HttpGet]
		public ActionResult<string> Get()
		{
			this.logfile.New(StandardLoglevel.Warning).Msg($"GET api/log {++count} time(s)").Log();
			return $"Logged {count} time(s).";
		}
	}
}
