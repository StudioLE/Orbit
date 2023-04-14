using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema;
using StudioLE.Core.Results;

namespace Orbit.Core;

public class Create
{
    public static IResult Execute(Instance instance)
    {
        instance.Review();
        {
            IResult result = Validate(instance);
            if (result is Failure failure)
                return failure;
        }
        {
            IResult result = Api.TryWriteInstance(instance, false);
            if (result is Failure failure)
                return failure;
        }
        return new Success();
    }

    private static IResult Validate(Instance instance)
    {
        List<ValidationResult> results = new();
        ValidationContext context = new(instance);
        return Validator.TryValidateObject(instance, context, results, validateAllProperties: true)
            ? new Success()
            : new Failure("The following validation errors occured:", results
                .Select(x => x.ErrorMessage)
                .OfType<string>()
                .ToArray());
    }
}
