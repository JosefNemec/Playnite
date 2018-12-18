namespace Playnite
{
    public enum ThreeStateFilterEnum
    {
        Disable,
        EnableInclusive, // OR/FULL JOIN
        EnableExclusive // AND/INNER JOIN
    }
}