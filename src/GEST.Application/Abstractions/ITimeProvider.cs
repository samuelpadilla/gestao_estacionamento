namespace GEST.Application.Abstractions;

public interface ITimeProvider
{
    DateTime UtcNow { get; }
}
