using System.Collections.Generic;

namespace Architect.Objects.Groups;

public static class GroupUtils
{
    public static List<T> Merge<T>(List<T> first, List<T> second)
    {
        foreach (var obj in first)
        {
            if (second.Contains(obj)) second.Remove(obj);
            second.Add(obj);
        }
        return second;
    }
}