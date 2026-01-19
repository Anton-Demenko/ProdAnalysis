using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProdAnalysis.Application.Options;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class DeviationEscalationWorker : BackgroundService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly DeviationOptions _options;

    public DeviationEscalationWorker(IDbContextFactory<AppDbContext> dbFactory, IOptions<DeviationOptions> options)
    {
        _dbFactory = dbFactory;
        _options = options.Value ?? new DeviationOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = _options.WorkerIntervalSeconds <= 0 ? 60 : _options.WorkerIntervalSeconds;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);
                await DeviationEventService.EvaluateEscalationsAsync(db, _options.EscalationMinutes);
            }
            catch
            {
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken);
            }
            catch
            {
            }
        }
    }
}