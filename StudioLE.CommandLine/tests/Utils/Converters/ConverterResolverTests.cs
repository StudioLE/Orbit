using NUnit.Framework;
using StudioLE.CommandLine.Tests.Resources;
using StudioLE.CommandLine.Utils.Converters;

namespace StudioLE.CommandLine.Tests.Utils.Converters;

internal sealed class ConverterResolverTests
{
    [TestCase(typeof(ExampleEnum), typeof(StringToEnum))]
    [TestCase(typeof(double), typeof(StringToDouble))]
    [TestCase(typeof(int), typeof(StringToInteger))]
    [TestCase(typeof(string), typeof(StringToString))]
    public void ConverterResolver_Resolve(Type sourceType, Type expectedConverterType)
    {
        // Arrange
        ConverterResolver resolver = ConverterResolver.Default();

        // Act
        Type? converter = resolver.Resolve(sourceType);

        // Assert
        Assert.That(converter, Is.Not.Null);
        Assert.That(converter, Is.EqualTo(expectedConverterType));
    }

    [TestCase(typeof(ExampleEnum), typeof(StringToEnum))]
    [TestCase(typeof(double), typeof(StringToDouble))]
    [TestCase(typeof(int), typeof(StringToInteger))]
    [TestCase(typeof(string), typeof(StringToString))]
    public void ConverterResolver_ResolveActivated(Type sourceType, Type expectedConverterType)
    {
        // Arrange
        ConverterResolver resolver = ConverterResolver.Default();

        // Act
        object? converter = resolver.ResolveActivated(sourceType);

        // Assert
        Assert.That(converter, Is.Not.Null);
        Assert.That(converter?.GetType(), Is.EqualTo(expectedConverterType));
    }

    [TestCase("Default", ExampleEnum.Default)]
    [TestCase("Alternative", ExampleEnum.Alternative)]
    [TestCase("INVALID", null)]
    public void ConverterResolver_ResolveActivated_Enum(string value, ExampleEnum? expected)
    {
        // Arrange
        ConverterResolver resolver = ConverterResolver.Default();
        object? converter = resolver.ResolveActivated(typeof(ExampleEnum));
        if (converter is not StringToEnum stringToEnum)
            throw new("Resolved incorrect type");

        // Act
        Enum? result = stringToEnum.Convert(value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}
