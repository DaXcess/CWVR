using BepInEx.Logging;

namespace CWVR.Preload;

public class BepInExLogger : ILogger
{
    private static ManualLogSource logSource = Logger.CreateLogSource("CWVR.Preload");
    
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

public class NopLogger : ILogger
{
    public void Log(object message)
    {
    }

    public void LogInfo(object message)
    {
    }

    public void LogWarning(object message)
    {
    }

    public void LogError(object message)
    {
    }

    public void LogDebug(object message)
    {
    }
}

public interface ILogger
{
    public void Log(object message);

    public void LogInfo(object message);

    public void LogWarning(object message);

    public void LogError(object message);

    public void LogDebug(object message);
}