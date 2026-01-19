using System.Text;
using Microsoft.EntityFrameworkCore;
using ProdAnalysis.Application.Dtos.Integrations;
using ProdAnalysis.Application.Services.Interfaces;
using ProdAnalysis.Domain.Entities;
using ProdAnalysis.Domain.Enums;
using ProdAnalysis.Infrastructure.Persistence;

namespace ProdAnalysis.Infrastructure.Services;

public sealed class CsvIntegrationService : ICsvIntegrationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CsvIntegrationService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<byte[]> ExportProductionDayHourlyCsvAsync(Guid productionDayId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var day = await db.ProductionDays
            .AsNoTracking()
            .Include(x => x.WorkCenter)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == productionDayId);

        if (day == null)
            throw new InvalidOperationException("ProductionDay not found.");

        var hours = await db.HourlyRecords
            .AsNoTracking()
            .Include(x => x.HourlyDowntimes)
            .Where(x => x.ProductionDayId == productionDayId)
            .OrderBy(x => x.HourIndex)
            .ToListAsync();

        var sb = new StringBuilder();

        sb.AppendLine("ProductionDayId,Date,WorkCenter,Product,ShiftStart,ShiftEnd");
        sb.AppendLine(string.Join(",",
            Esc(day.Id.ToString()),
            Esc(day.Date.ToString("yyyy-MM-dd")),
            Esc(day.WorkCenter.Name),
            Esc(day.Product.Name),
            Esc(day.ShiftStart.ToString("HH:mm")),
            Esc(day.ShiftEnd.ToString("HH:mm"))
        ));

        sb.AppendLine();
        sb.AppendLine("HourIndex,HourStart,PlanQty,ActualQty,DowntimeMinutes,Comment");

        foreach (var h in hours)
        {
            var dt = h.HourlyDowntimes.Sum(x => x.Minutes);
            var actual = (h.ActualQty ?? 0).ToString();
            var comment = h.Comment ?? "";

            sb.AppendLine(string.Join(",",
                h.HourIndex.ToString(),
                Esc(h.HourStart.ToString("HH:mm")),
                h.PlanQty.ToString(),
                Esc(actual),
                dt.ToString(),
                Esc(comment)
            ));
        }

        return WithUtf8Bom(sb.ToString());
    }

    public async Task<CsvImportResultDto> ImportProductionDayHourlyCsvAsync(Guid productionDayId, Stream csvStream, Guid userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var day = await db.ProductionDays
            .Include(x => x.WorkCenter)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == productionDayId);

        if (day == null)
            throw new InvalidOperationException("ProductionDay not found.");

        var hours = await db.HourlyRecords
            .Include(x => x.ProductionDay)
            .Where(x => x.ProductionDayId == productionDayId)
            .ToListAsync();

        var map = hours.ToDictionary(x => x.HourIndex);

        using var sr = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        var lines = new List<string>();
        while (true)
        {
            var line = await sr.ReadLineAsync();
            if (line == null)
                break;
            lines.Add(line);
        }

        var errors = new List<string>();
        var updated = 0;
        var skipped = 0;

        var headerIndex = FindHeaderLineIndex(lines, "HourIndex");
        if (headerIndex < 0)
            throw new InvalidOperationException("CSV must contain an 'HourIndex' header.");

        if (headerIndex + 1 >= lines.Count)
            return new CsvImportResultDto(0, 0, Array.Empty<string>());

        var header = ParseCsvLine(lines[headerIndex]);
        var colHourIndex = FindColumn(header, "HourIndex");
        var colActual = FindColumn(header, "ActualQty");
        var colComment = FindColumn(header, "Comment");

        if (colHourIndex < 0)
            throw new InvalidOperationException("CSV must contain 'HourIndex' column.");
        if (colActual < 0)
            throw new InvalidOperationException("CSV must contain 'ActualQty' column.");

        for (var i = headerIndex + 1; i < lines.Count; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw))
            {
                skipped++;
                continue;
            }

            var row = ParseCsvLine(raw);

            if (colHourIndex >= row.Count)
            {
                errors.Add($"Line {i + 1}: missing HourIndex.");
                skipped++;
                continue;
            }

            if (!int.TryParse(row[colHourIndex], out var hourIndex))
            {
                errors.Add($"Line {i + 1}: invalid HourIndex '{row[colHourIndex]}'.");
                skipped++;
                continue;
            }

            if (!map.TryGetValue(hourIndex, out var hr))
            {
                errors.Add($"Line {i + 1}: HourIndex {hourIndex} not found for this ProductionDay.");
                skipped++;
                continue;
            }

            var actualText = colActual < row.Count ? row[colActual] : "";
            if (!int.TryParse(actualText, out var actual) || actual < 0)
            {
                errors.Add($"Line {i + 1}: invalid ActualQty '{actualText}'.");
                skipped++;
                continue;
            }

            var comment = colComment >= 0 && colComment < row.Count ? row[colComment] : null;
            comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

            var changed = (hr.ActualQty ?? 0) != actual || hr.Comment != comment;

            hr.ActualQty = actual;
            hr.Comment = comment;
            hr.UpdatedAt = DateTime.UtcNow;
            hr.UpdatedByUserId = userId;

            await UpsertDeviationEventAsync(db, hr, userId);

            if (changed)
                updated++;
            else
                skipped++;
        }

        await db.SaveChangesAsync();

        return new CsvImportResultDto(updated, skipped, errors);
    }

    public async Task<byte[]> ExportParetoCsvAsync(DateOnly from, DateOnly to, Guid? workCenterId)
    {
        if (to < from)
            (from, to) = (to, from);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var rows = await db.HourlyDowntimes
            .AsNoTracking()
            .Where(x => x.HourlyRecord.ProductionDay.Date >= from && x.HourlyRecord.ProductionDay.Date <= to)
            .Where(x => !workCenterId.HasValue || x.HourlyRecord.ProductionDay.WorkCenterId == workCenterId.Value)
            .GroupBy(x => x.DowntimeReason.Name)
            .Select(g => new
            {
                Reason = g.Key,
                Minutes = g.Sum(x => x.Minutes)
            })
            .ToListAsync();

        var ordered = rows
            .OrderByDescending(x => x.Minutes)
            .ThenBy(x => x.Reason)
            .ToList();

        var total = ordered.Sum(x => x.Minutes);
        if (total <= 0)
            total = 0;

        string wcName = "All";
        if (workCenterId.HasValue)
        {
            var wc = await db.WorkCenters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workCenterId.Value);
            if (wc != null)
                wcName = wc.Name;
        }

        var sb = new StringBuilder();

        sb.AppendLine("Report,Pareto Downtime");
        sb.AppendLine($"From,{Esc(from.ToString("yyyy-MM-dd"))}");
        sb.AppendLine($"To,{Esc(to.ToString("yyyy-MM-dd"))}");
        sb.AppendLine($"WorkCenter,{Esc(wcName)}");
        sb.AppendLine($"TotalMinutes,{total}");
        sb.AppendLine();

        sb.AppendLine("Reason,Minutes,Percent,CumPercent");

        decimal cum = 0m;

        foreach (var it in ordered)
        {
            decimal pct = total == 0 ? 0m : (decimal)it.Minutes * 100m / total;
            cum += pct;

            sb.AppendLine(string.Join(",",
                Esc(it.Reason),
                it.Minutes.ToString(),
                pct.ToString("0.##"),
                cum.ToString("0.##")
            ));
        }

        return WithUtf8Bom(sb.ToString());
    }

    private static int FindHeaderLineIndex(List<string> lines, string mustContain)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var t = lines[i].Trim();
            if (t.Length == 0)
                continue;

            if (t.Contains(mustContain, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static int FindColumn(List<string> header, string name)
    {
        for (var i = 0; i < header.Count; i++)
        {
            if (string.Equals(header[i], name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static byte[] WithUtf8Bom(string text)
    {
        var payload = Encoding.UTF8.GetBytes(text);
        var bom = Encoding.UTF8.GetPreamble();
        if (bom.Length == 0)
            return payload;

        var res = new byte[bom.Length + payload.Length];
        Buffer.BlockCopy(bom, 0, res, 0, bom.Length);
        Buffer.BlockCopy(payload, 0, res, bom.Length, payload.Length);
        return res;
    }

    private static string Esc(string s)
    {
        if (s.Contains('"'))
            s = s.Replace("\"", "\"\"");

        if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"'))
            return $"\"{s}\"";

        return s;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var res = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == ',')
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        res.Add(sb.ToString());
        return res;
    }

    private static async Task UpsertDeviationEventAsync(AppDbContext db, HourlyRecord hr, Guid userId)
    {
        var actual = hr.ActualQty ?? 0;
        var plan = hr.PlanQty;

        var open = await db.DeviationEvents
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.HourlyRecordId == hr.Id && x.Status != DeviationEventStatus.Closed);

        if (actual < plan)
        {
            if (open == null)
            {
                var ev = new DeviationEvent
                {
                    Id = Guid.NewGuid(),
                    ProductionDayId = hr.ProductionDayId,
                    HourlyRecordId = hr.Id,
                    WorkCenterId = hr.ProductionDay.WorkCenterId,
                    ProductId = hr.ProductionDay.ProductId,
                    ProductionDate = hr.ProductionDay.Date,
                    HourIndex = hr.HourIndex,
                    HourStart = hr.HourStart,
                    PlanQty = plan,
                    ActualQty = actual,
                    DeviationQty = actual - plan,
                    Status = DeviationEventStatus.Open,
                    CurrentEscalationLevel = 1,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId,
                    Note = null
                };

                db.DeviationEvents.Add(ev);

                db.EscalationLogs.Add(new EscalationLog
                {
                    Id = Guid.NewGuid(),
                    DeviationEventId = ev.Id,
                    Level = 1,
                    CreatedAt = DateTime.UtcNow,
                    Message = "Deviation detected (L1: Master)."
                });

                return;
            }

            open.PlanQty = plan;
            open.ActualQty = actual;
            open.DeviationQty = actual - plan;

            if (open.Status == DeviationEventStatus.Closed)
                open.Status = DeviationEventStatus.Open;

            return;
        }

        if (open != null)
        {
            open.Status = DeviationEventStatus.Closed;
            open.ClosedAt = DateTime.UtcNow;
            open.ClosedByUserId = userId;

            db.EscalationLogs.Add(new EscalationLog
            {
                Id = Guid.NewGuid(),
                DeviationEventId = open.Id,
                Level = Math.Max(1, open.CurrentEscalationLevel),
                CreatedAt = DateTime.UtcNow,
                Message = "Auto-closed: plan met."
            });
        }
    }
}