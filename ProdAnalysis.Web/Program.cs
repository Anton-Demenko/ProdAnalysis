using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Options;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Infrastructure.Persistence;
using ProdAnalysis.Infrastructure.Services;
using ProdAnalysis.Web.Components;
using ProdAnalysis.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<DeviationOptions>(builder.Configuration.GetSection("Deviations"));

var connectionString = builder.Configuration.GetConnectionString("Sqlite");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlite(connectionString, x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name));
});

builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IProductionDayService, ProductionDayService>();
builder.Services.AddScoped<IHourlyRecordService, HourlyRecordService>();
builder.Services.AddScoped<IDowntimeService, DowntimeService>();
builder.Services.AddScoped<IDeviationEventService, DeviationEventService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICsvIntegrationService, CsvIntegrationService>();
builder.Services.AddScoped<IWorkCenterAdminService, WorkCenterAdminService>();
builder.Services.AddScoped<IProductAdminService, ProductAdminService>();

builder.Services.AddHostedService<DeviationEscalationWorker>();

builder.Services.AddScoped<CurrentUserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);

    var demoSeed = app.Configuration.GetValue<bool>("DemoSeed");
    var demoSeedReset = app.Configuration.GetValue<bool>("DemoSeedReset");

    app.Logger.LogInformation("DemoSeed enabled: {DemoSeed}, reset: {DemoSeedReset}, env: {Env}", demoSeed, demoSeedReset, app.Environment.EnvironmentName);

    if (app.Environment.IsDevelopment() && demoSeed)
        await DemoSeeder.SeedAsync(db, demoSeedReset);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/api/csv/export/production-day/{id:guid}", async (Guid id, ICsvIntegrationService csv) =>
{
    var bytes = await csv.ExportProductionDayHourlyCsvAsync(id);
    return Results.File(bytes, "text/csv", $"production_day_{id}.csv");
});

app.MapGet("/api/csv/export/pareto", async (HttpRequest request, ICsvIntegrationService csv) =>
{
    var fromText = request.Query["from"].ToString();
    var toText = request.Query["to"].ToString();
    var wcText = request.Query["workCenterId"].ToString();

    if (!DateOnly.TryParse(fromText, out var from))
        return Results.BadRequest("Invalid 'from' (expected yyyy-MM-dd).");

    if (!DateOnly.TryParse(toText, out var to))
        return Results.BadRequest("Invalid 'to' (expected yyyy-MM-dd).");

    Guid? wcId = null;
    if (!string.IsNullOrWhiteSpace(wcText))
    {
        if (Guid.TryParse(wcText, out var g))
            wcId = g;
        else
            return Results.BadRequest("Invalid 'workCenterId'.");
    }

    var bytes = await csv.ExportParetoCsvAsync(from, to, wcId);

    var wcPart = wcId.HasValue ? "_wc" : "";
    var name = $"pareto_{from:yyyyMMdd}_{to:yyyyMMdd}{wcPart}.csv";

    return Results.File(bytes, "text/csv", name);
});

app.MapGet("/api/csv/export/summary", async (HttpRequest request, ICsvIntegrationService csv) =>
{
    var fromText = request.Query["from"].ToString();
    var toText = request.Query["to"].ToString();
    var wcText = request.Query["workCenterId"].ToString();

    if (!DateOnly.TryParse(fromText, out var from))
        return Results.BadRequest("Invalid 'from' (expected yyyy-MM-dd).");

    if (!DateOnly.TryParse(toText, out var to))
        return Results.BadRequest("Invalid 'to' (expected yyyy-MM-dd).");

    Guid? wcId = null;
    if (!string.IsNullOrWhiteSpace(wcText))
    {
        if (Guid.TryParse(wcText, out var g))
            wcId = g;
        else
            return Results.BadRequest("Invalid 'workCenterId'.");
    }

    var bytes = await csv.ExportProductionSummaryCsvAsync(from, to, wcId);

    var wcPart = wcId.HasValue ? "_wc" : "";
    var name = $"summary_{from:yyyyMMdd}_{to:yyyyMMdd}{wcPart}.csv";

    return Results.File(bytes, "text/csv", name);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
