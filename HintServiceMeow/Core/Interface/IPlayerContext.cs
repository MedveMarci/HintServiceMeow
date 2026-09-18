using System;

namespace HintServiceMeow.Core.Interface;

internal interface IPlayerContext : IEquatable<IPlayerContext>
{
    bool IsValid();
}