using System;
using Hints;
using HintServiceMeow.Core.Interface;
using Mirror;

namespace HintServiceMeow.Core.Models.UnityAdaptors.Parameters;

public class TimespanParameter(double sourceTime, string format, bool negate) : IParameter
{
    public double SourceTime { get; set; } = sourceTime;

    public string Format { get; set; } = format;

    public bool Negate { get; set; } = negate;

    public TimespanParameter(DateTimeOffset sourceTime, string format, bool negate) : this((sourceTime - DateTimeOffset.UtcNow).TotalSeconds, format, negate)
    { }

    public static TimespanParameter FromOffset(double offset, string format, bool negate)
    {
        return new TimespanParameter(NetworkTime.time + offset, format, negate);
    }

    public static TimespanParameter FromOffset(TimeSpan offset, string format, bool negate)
    {
        return FromOffset(offset.TotalSeconds, format, negate);
    }

    public HintParameter GetScpslHintParameter()
    {
        return new TimespanHintParameter(SourceTime, Format, Negate);
    }
}