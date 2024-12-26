namespace CWVR.MultiLoader.Common;

public interface ILogger
{
    public void Log(object message);

    public void LogInfo(object message);

    public void LogWarning(object message);

    public void LogError(object message);

    public void LogDebug(object message);
}