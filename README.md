# Logfile.AspNetCore

This projects implements an ASP.NET Core adaption for the fluent and detail-retaining Logfile logging library. This enables forwarding of all framework-generated log events to the configured Logfile loggers. Thus, any library may use the `ILogger` interface of the ASP.NET Core framework as well as the more sophisticated Logfile framework to accomplish complex logging tasks.