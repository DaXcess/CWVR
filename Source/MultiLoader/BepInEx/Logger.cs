using BepInEx.Logging;
using CWVR.MultiLoader.Common;

namespace CWVR.MultiLoader.BepInEx;

public class Logger(ManualLogSource logSource) : ILogger
{
    public void Log(object message)
    {
        logSource.LogInfo(message);
    }

    public void LogInfo(object message)
    {
        logSource.LogInfo(message);
    }

    public void LogWarning(object message)
    {
        logSource.LogWarning(message);
    }

    public void LogError(object message)
    {
        logSource.LogError(message);
    }

    public void LogDebug(object message)
    {
        logSource.LogDebug(message);
    }
}