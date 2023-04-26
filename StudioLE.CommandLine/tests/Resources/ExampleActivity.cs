using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using StudioLE.Core.System;

namespace StudioLE.CommandLine.Tests.Resources;

public class ExampleActivity
{
    private readonly ILogger<ExampleActivity> _logger;

    public ExampleActivity(ILogger<ExampleActivity> logger)
    {
        _logger = logger;
    }

    public ExampleClass Execute(ExampleClass example)
    {
        List<ValidationResult> results = new();
        ValidationContext context = new(example);
        if(Validator.TryValidateObject(example, context, results, validateAllProperties: true))
            _logger.LogError(results
                .Select(x => x.ErrorMessage)
                .OfType<string>()
                .Join());
        return example;
    }
}
