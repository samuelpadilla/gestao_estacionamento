using FluentValidation;
using GEST.Application.Dtos.Webhook;

namespace GEST.Application.Validation.Webhook;

public sealed class ExitEventValidator : AbstractValidator<ExitEventDto>
{
    public ExitEventValidator()
    {
        RuleFor(x => x.Event_Type).Equal("EXIT");
        RuleFor(x => x.License_Plate).NotEmpty().MaximumLength(12);
        RuleFor(x => x.Exit_Time).NotEmpty();
    }
}
