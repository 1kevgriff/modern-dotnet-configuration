using Microsoft.Extensions.Logging;

namespace D26.NonWebHosts.Worker;

/// <summary>
/// Source-generated log methods. Warnings-as-errors plus CA1848 means the demo writes logging the
/// way a real worker should. Explicit event ids keep the console prefix short on a projector.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Host           {Host}")]
    public static partial void HostKind(ILogger logger, string host);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Environment    {Environment} (DOTNET_ENVIRONMENT, not ASPNETCORE_)")]
    public static partial void Environment(ILogger logger, string environment);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Endpoint       {Endpoint}")]
    public static partial void Endpoint(ILogger logger, string endpoint);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Timeout        {Timeout}s")]
    public static partial void Timeout(ILogger logger, int timeout);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Provider       {Provider}")]
    public static partial void Provider(ILogger logger, string provider);
}
