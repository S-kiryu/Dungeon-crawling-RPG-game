using UnityEngine;

public interface IRandomSource
{
    int Range(int minimumInclusive, int maximumExclusive);
    float Range(float minimumInclusive, float maximumInclusive);
}

public sealed class UnityRandomSource : IRandomSource
{
    public int Range(int minimumInclusive, int maximumExclusive)
    {
        return Random.Range(minimumInclusive, maximumExclusive);
    }

    public float Range(float minimumInclusive, float maximumInclusive)
    {
        return Random.Range(minimumInclusive, maximumInclusive);
    }
}
