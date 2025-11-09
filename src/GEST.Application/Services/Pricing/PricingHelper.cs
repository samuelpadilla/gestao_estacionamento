using GEST.Domain.Enums;

namespace GEST.Application.Services.Pricing;

internal static class PricingHelper
{
    public static (PricingTier tier, decimal multiplier) DecideDynamicPrice(int occupiedCount, int maxCapacity)
    {
        if (maxCapacity <= 0)
            throw new InvalidOperationException("Setor sem capacidade.");

        var occ = (decimal)occupiedCount / maxCapacity;

        if (occ < 0.25m)
            return (PricingTier.Discount10, 0.90m);

        if (occ <= 0.50m)
            return (PricingTier.Normal, 1.00m);

        if (occ <= 0.75m)
            return (PricingTier.Plus10, 1.10m);

        return (PricingTier.Plus25, 1.25m);
    }

    public static decimal ComputeAmount(
        DateTime entryUtc,
        DateTime exitUtc,
        decimal pricePerHour)
    {
        var totalMinutes = (exitUtc - entryUtc).TotalMinutes;

        if (totalMinutes <= 30)
            return 0m;

        var billableMinutes = totalMinutes - 30;
        var hours = (int)Math.Ceiling(billableMinutes / 60d);
        return pricePerHour * hours;
    }
}
