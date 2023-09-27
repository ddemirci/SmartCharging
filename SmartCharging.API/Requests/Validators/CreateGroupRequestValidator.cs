using FluentValidation;
using SmartCharging.API.Requests.Group;

namespace SmartCharging.API.Requests.Validators;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.CapacityInAmps).GreaterThan(0)
            .WithMessage("CapacityInAmps have to be an integer and greater than 0.");
    }
}