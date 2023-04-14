using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema;
using StudioLE.Core.Results;
using StudioLE.Core.System;

namespace Orbit.Core;

public class Create
{
    public static IResult<Instance> Execute(Instance instance)
    {
        instance.Review();

        List<ValidationResult> results = new();
        ValidationContext context = new(instance);
        bool isValid =  Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        if (!isValid)
        {
            string[] errors = results
                .Select(x => x.MemberNames.Join(", ") + ":" + x.ErrorMessage)
                .ToArray();
            return new Failure<Instance>("The following validation errors occured.", errors);
        }

        IResult result = Api.TryWriteInstance(instance, false);
        if(result is not Success)
            return new Failure<Instance>(result.Errors)
            {
                Warnings = result.Warnings
            };

        return new Success<Instance>(instance);
    }
}
