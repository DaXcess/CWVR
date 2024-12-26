namespace CWVR;

public static class Logger
{
    public static void Log(object message)
    {
        Plugin.Logger.Log(message);
    }

    public static void LogInfo(object message)
    {
        Plugin.Logger.LogInfo(message);
    }

    public static void LogWarning(object message)
    {
        Plugin.Logger.LogWarning(message);
    }

    public static void LogError(object message)
    {
        Plugin.Logger.LogError(message);
    }

    public static void LogDebug(object message)
    {
        Plugin.Logger.LogDebug(message);
    }
}