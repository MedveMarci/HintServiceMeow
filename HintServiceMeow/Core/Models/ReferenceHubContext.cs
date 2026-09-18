using System;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models;

internal class ReferenceHubContext(ReferenceHub referenceHub) : IPlayerContext
{
    public ReferenceHub? ReferenceHub { get; } = referenceHub ?? throw new ArgumentNullException(nameof(referenceHub), "ReferenceHub cannot be null");

    public static bool operator ==(ReferenceHubContext? left, ReferenceHubContext? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(ReferenceHubContext left, ReferenceHubContext right)
    {
        return !(left == right);
    }

    public bool IsValid()
    {
        return ReferenceHub && ReferenceHub.connectionToClient != null;
    }

    public bool Equals(IPlayerContext other)
    {
        if (other is ReferenceHubContext otherContext) return ReferenceHub == otherContext.ReferenceHub;

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is IPlayerContext context) return Equals(context);

        return false;
    }

    public override int GetHashCode()
    {
        return ReferenceHub?.GetHashCode() ?? 0;
    }
}