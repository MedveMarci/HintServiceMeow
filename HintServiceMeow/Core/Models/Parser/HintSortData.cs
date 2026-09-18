using System;
using HintServiceMeow.Core.Models.Hints;

internal readonly struct HintSortData(Hint hint, float y) : IComparable<HintSortData>
{
    public readonly Hint Hint = hint;
    public readonly float Y = y;

    public int CompareTo(HintSortData other)
    {
        return Y.CompareTo(other.Y);
    }
}