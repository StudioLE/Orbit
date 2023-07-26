namespace Orbit.Core.Generation;

public class WriteFileCommandFactory
{
    public string Create(string path, string content)
    {
        return $$"""
            (
            cat <<EOF
            {{content}}
            EOF
            ) | tee "{{path}}"
            """;
    }
}
