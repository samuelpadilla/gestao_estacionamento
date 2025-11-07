using GEST.Application.Abstractions;

namespace GEST.Application.Services.Time;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
