namespace Orbit.Core.Schema;

public class IdHelpers
{
    public static int GetClusterNumber(string id)
    {
        string number = id.Split("-").First();
        return int.Parse(number);
    }

    public static int GetInstanceNumber(string id)
    {
        string number = id.Split("-").Last();
        return int.Parse(number);
    }
}
