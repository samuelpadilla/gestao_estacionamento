using FluentValidation;
using GEST.Application.Dtos.Webhook;

namespace GEST.Application.Validation.Webhook;

public sealed class EntryEventValidator : AbstractValidator<EntryEventDto>
{
    public EntryEventValidator()
    {
        RuleFor(x => x.Event_Type).Equal("ENTRY");
        RuleFor(x => x.License_Plate).NotEmpty().MaximumLength(12);
        RuleFor(x => x.Entry_Time).NotEmpty();
    }
}
