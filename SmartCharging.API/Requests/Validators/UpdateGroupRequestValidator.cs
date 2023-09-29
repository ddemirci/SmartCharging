using FluentValidation;
using SmartCharging.API.Requests.Group;

namespace SmartCharging.API.Requests.Validators;

public class UpdateGroupRequestValidator: AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .Must(x=> !string.IsNullOrWhiteSpace(x)).When(x=> x.Name != null)
            .WithMessage("Name should not be empty");
        RuleFor(x => x.CapacityInAmps)
            .Must(x=> x > 0).When(x=> x.CapacityInAmps.HasValue)
            .WithMessage("CapacityInAmps have to be an integer and greater than 0.");
    }
}