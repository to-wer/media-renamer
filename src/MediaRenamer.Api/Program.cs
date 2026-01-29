using MediaRenamer.Api.Background;
using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Providers;
using MediaRenamer.Core.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddSingleton<IMediaScanner, MediaScanner>();
    builder.Services.AddSingleton<IMetadataProvider>(_ => new TmdbMetadataProvider(
            builder.Configuration["TMDB:ApiKey"]!
        )
    );
    builder.Services.AddSingleton<MetadataResolver>();
    builder.Services.AddSingleton<IRenameService, RenameService>();
    builder.Services.AddSingleton<ProposalStore>();

    builder.Services.AddHostedService<MediaWatcherService>();


    var app = builder.Build();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}