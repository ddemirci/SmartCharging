using FluentValidation;
using SmartCharging.API.Requests.Connector;

namespace SmartCharging.API.Requests.Validators;

public class CreateConnectorRequestValidator : AbstractValidator<CreateConnectorRequest>
{
    public CreateConnectorRequestValidator()
    {
        RuleFor(x => x.MaxCurrentInAmps).GreaterThan(0)
            .WithMessage("MaxCurrentInAmps have to be an integer and greater than 0.");
    }
}