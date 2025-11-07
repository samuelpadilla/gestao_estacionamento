using FluentValidation;
using GEST.Application.Dtos.Webhook;

namespace GEST.Application.Validation.Webhook;

public sealed class ParkedEventValidator : AbstractValidator<ParkedEventDto>
{
    public ParkedEventValidator()
    {
        RuleFor(x => x.Event_Type).Equal("PARKED");
        RuleFor(x => x.License_Plate).NotEmpty().MaximumLength(12);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
    }
}
