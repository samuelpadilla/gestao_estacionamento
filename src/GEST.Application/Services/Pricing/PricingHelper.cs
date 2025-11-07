using GEST.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEST.Application.Services.Pricing;

internal static class PricingHelper
{
    public static (PricingTier tier, decimal price) DecideDynamicPrice(
        int occupiedCount, int maxCapacity, decimal basePrice)
    {
        if (maxCapacity <= 0) throw new InvalidOperationException("Setor sem capacidade.");
        var occ = (decimal)occupiedCount / maxCapacity;

        if (occ < 0.25m) return (PricingTier.Discount10, basePrice * 0.90m);
        if (occ <= 0.50m) return (PricingTier.Normal, basePrice);
        if (occ <= 0.75m) return (PricingTier.Plus10, basePrice * 1.10m);
        return (PricingTier.Plus25, basePrice * 1.25m);
    }

    public static decimal ComputeAmount(DateTime entryUtc, DateTime exitUtc, decimal pricePerHour)
    {
        var totalMinutes = (exitUtc - entryUtc).TotalMinutes;
        if (totalMinutes <= 30) return 0m;

        var billableMinutes = totalMinutes - 30;
        var hours = (int)Math.Ceiling(billableMinutes / 60d);
        return pricePerHour * hours;
    }
}
