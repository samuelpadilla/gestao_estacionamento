using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEST.Domain.Enums;

public enum PricingTier
{
    Discount10 = 0,  // Lotação < 25%
    Normal = 1,      // ≤ 50%
    Plus10 = 2,      // ≤ 75%
    Plus25 = 3       // ≤ 100%
}
