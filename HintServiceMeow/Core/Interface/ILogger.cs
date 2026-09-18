namespace HintServiceMeow.Core.Interface;

/// <summary>
///     Defines a logging abstraction for recording informational, error, and debug messages.
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Gets a value indicating whether debug logging is currently enabled.
    /// </summary>
    /// <remarks>
    ///     Call sites in hot paths should check this before building an interpolated debug
    ///     message, so the (potentially large) message string is never allocated when debug
    ///     logging is turned off.
    /// </remarks>
    bool IsDebugEnabled { get; }

    /// <summary>
    ///     Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Info(object message);

    /// <summary>
    ///     Logs an error message.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    void Error(object message);

    /// <summary>
    ///     Logs a debug message.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    void Debug(object message);
}