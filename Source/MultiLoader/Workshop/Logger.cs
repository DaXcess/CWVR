using UnityEngine;
using ILogger = CWVR.MultiLoader.Common.ILogger;

namespace CWVR.MultiLoader.Workshop;

public class Logger : ILogger
{
    public void Log(object message)
    {
        Debug.Log(message);
    }

    public void LogInfo(object message)
    {
        Debug.Log(message);
    }

    public void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }

    public void LogError(object message)
    {
        Debug.LogError(message);
    }

    public void LogDebug(object message)
    {
        Debug.Log(message);
    }
}