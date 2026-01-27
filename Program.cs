using EliosBrokerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

try
{
    // Crea una configurazione preliminare per inizializzare Serilog
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Inizializza Serilog con la configurazione da appsettings.json
    Log.Logger = new LoggerConfiguration()
        .ReadFrom
        .Configuration(configuration)
        .CreateLogger();

    // Inizializza anche EBLogger
    //EBLogger.Init(configuration);

    Log.Information("=== Avvio EliosBrokerService ===");
    Log.Information("Directory applicazione: {BaseDirectory}", AppDomain.CurrentDomain.BaseDirectory);

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = ".NET EliosBrokerService";
    });

    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    // Rimuovi i logger di default e usa solo Serilog
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog(Log.Logger, dispose: true);

    builder.Services.AddHostedService<QueueWorker>();

    IHost host = builder.Build();

    Log.Information("Host creato con successo, avvio servizio...");

    await host.RunAsync();

    Log.Information("=== Servizio arrestato normalmente ===");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Errore fatale durante l'avvio del servizio");    
    throw;
}
finally
{
    Log.CloseAndFlush();    
}