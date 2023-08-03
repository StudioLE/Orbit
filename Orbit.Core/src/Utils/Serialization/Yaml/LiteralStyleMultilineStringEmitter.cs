using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Orbit.Core.Utils.Serialization.Yaml;

/// <summary>
/// Set the default scalar style for multiline strings to <see cref="ScalarStyle.Literal"/>.
/// </summary>
/// <seealso href="https://stackoverflow.com/a/58638680/247218"/>
public class LiteralStyleMultilineStringEmitter : ChainedEventEmitter
{
    internal LiteralStyleMultilineStringEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
    {
    }

    /// <inheritdoc/>
    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (eventInfo.Style == ScalarStyle.Any && IsMultiLineString(eventInfo.Source))
            eventInfo.Style = ScalarStyle.Literal;
        nextEmitter.Emit(eventInfo, emitter);
    }

    private static bool IsMultiLineString(IObjectDescriptor descriptor)
    {
        if (descriptor.Type != typeof(string))
            return false;
        string? value = descriptor.Value as string;
        if (string.IsNullOrEmpty(value))
            return false;
        char[] lineBreakCharacters = { '\r', '\n', '\x85', '\x2028', '\x2029' };
        return value.IndexOfAny(lineBreakCharacters) >= 0;
    }
}

/// <summary>
/// Methods to extend <see cref="SerializerBuilder"/>.
/// </summary>
public static class SerializerBuilderExtensions
{
    /// <summary>
    /// Set the default scalar style for multiline strings to <see cref="ScalarStyle.Literal"/>.
    /// </summary>
    public static SerializerBuilder WithLiteralMultilineStrings(this SerializerBuilder builder)
    {
        return builder.WithEventEmitter(nextEmitter => new LiteralStyleMultilineStringEmitter(nextEmitter));
    }
}
