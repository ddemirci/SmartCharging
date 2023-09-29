using FluentValidation;
using SmartCharging.API.Requests.ChargeStation;

namespace SmartCharging.API.Requests.Validators;

public class CreateChargeStationRequestValidator: AbstractValidator<CreateChargeStationRequest>
{
    public CreateChargeStationRequestValidator()
    {
        RuleFor(x => x.ConnectorMaxCurrentInAmps).GreaterThan(0)
            .WithMessage("ConnectorMaxCurrentInAmps have to be an integer and greater than 0.");
    }
}