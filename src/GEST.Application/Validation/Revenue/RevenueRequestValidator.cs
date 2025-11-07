using FluentValidation;
using GEST.Application.Dtos.Revenue;

namespace GEST.Application.Validation.Revenue;

public sealed class RevenueRequestValidator : AbstractValidator<RevenueRequestDto>
{
    public RevenueRequestValidator()
    {
        RuleFor(x => x.Sector).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Date).NotEmpty();
    }
}
