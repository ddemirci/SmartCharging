using FluentValidation;
using SmartCharging.API.Requests.Group;

namespace SmartCharging.API.Requests.Validators;

public class UpdateConnectorRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateConnectorRequestValidator()
    {
        RuleFor(x => x.CapacityInAmps).GreaterThan(0)
            .WithMessage("CapacityInAmps have to be an integer and greater than 0.");
    }
}