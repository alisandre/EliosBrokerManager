using EliosBrokerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = ".NET EliosBrokerService";
});

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddHostedService<QueueWorker>();

builder.Logging.AddConfiguration();

IHost host = builder.Build();

host.Run();