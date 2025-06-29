using System.Globalization;
using TournamentManager.DI;

namespace TournamentManager.ModelValidators;

public class PhoneNumberValidator : AbstractValidator<string, (PhoneNumberService PhoneNumberService, RegionInfo Region), PhoneNumberValidator.FactId>
{
    public enum FactId
    {
        NumberIsValid
    }

    public PhoneNumberValidator(string model, (PhoneNumberService PhoneNumberService, RegionInfo Region) data) : base(model, data)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        Facts.Add(NumberIsValid());
    }

    private Fact<FactId> NumberIsValid()
    {
        return new Fact<FactId>
        {
            Id = FactId.NumberIsValid,
            FieldNames = ["PhoneNumber"],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = PhoneNumberValidatorResource.NumberIsValid,
                    Success = Data.PhoneNumberService.IsValid(Model, Data.Region.TwoLetterISORegionName)
                })
        };
    }
}
