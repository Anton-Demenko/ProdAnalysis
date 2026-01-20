namespace ProdAnalysis.Infrastructure.Services.Planning;

public static class ShiftPlanCalculator
{
    public static int[] CalculateUniformHourlyPlan(int taktSec, int hours)
    {
        if (hours < 1)
            throw new InvalidOperationException("Hours must be >= 1.");

        if (taktSec < 1)
            throw new InvalidOperationException("TaktSec must be >= 1.");

        var planPerHour = 3600 / taktSec;

        var plan = new int[hours];
        for (var i = 0; i < hours; i++)
            plan[i] = planPerHour;

        return plan;
    }

    public static int[] CalculateCumulativeHourlyPlan(int taktSec, int hours)
    {
        if (hours < 1)
            throw new InvalidOperationException("Hours must be >= 1.");

        if (taktSec < 1)
            throw new InvalidOperationException("TaktSec must be >= 1.");

        var plan = new int[hours];
        var prevCum = 0;

        for (var i = 0; i < hours; i++)
        {
            var secondsToEndOfHour = (i + 1) * 3600m;
            var cum = (int)decimal.Floor(secondsToEndOfHour / taktSec);
            plan[i] = cum - prevCum;
            prevCum = cum;
        }

        return plan;
    }
}
