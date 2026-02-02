using MediaRenamer.Api.Background;
using MediaRenamer.Api.Data;
using MediaRenamer.Api.Services;
using MediaRenamer.Core.Abstractions;
using MediaRenamer.Core.Models;
using MediaRenamer.Core.Providers;
using MediaRenamer.Core.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((_, services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Database"));
    builder.Services.Configure<MediaSettings>(builder.Configuration.GetSection("Media"));

    builder.Services.AddSingleton<IFilenameParserService, FilenameParserService>();
    builder.Services.AddSingleton<IMediaScanner, MediaScanner>();

    // TMDB Provider nur registrieren, wenn API-Key vorhanden ist
    var tmdbApiKey = builder.Configuration["TMDb:ApiKey"];
    if (!string.IsNullOrWhiteSpace(tmdbApiKey) && tmdbApiKey != "your-tmdb-api-key-here")
    {
        builder.Services.AddSingleton<IMetadataProvider, TmdbMetadataProvider>();
        Log.Information("TMDB metadata provider registered (API key found)");
    }
    else
    {
        Log.Warning("TMDB API key not configured - running WITHOUT metadata enrichment");
    }

    builder.Services.AddSingleton<IMetadataProvider, NoOpMetadataProvider>();


    builder.Services.AddDbContext<ProposalDbContext>(options =>
    {
        var dbPath = builder.Configuration["Database:ProposalDbPath"]
                     ?? "/app/db/proposals.db";
        options.UseSqlite($"Data Source={dbPath}");
    });
    builder.Services.AddScoped<ProposalStore>();
    builder.Services.AddSingleton<MetadataResolver>();
    builder.Services.AddSingleton<IRenameService, RenameService>();

    builder.Services.AddHostedService<MediaWatcherService>();

    builder.Services.AddControllers();

    var app = builder.Build();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.MapControllers();
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