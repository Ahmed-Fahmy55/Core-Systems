using UnityEngine;

public static class Logger
{
    public static void Log(object log, UnityEngine.Object contx = null)
    {
#if EnableLogs || UNITY_EDITOR
        if (contx == null)
        {
            Debug.Log(log);
        }
        else Debug.Log(log, contx);
#endif
    }

    public static void LogError(object log, UnityEngine.Object contx = null)
    {
#if EnableLogs || UNITY_EDITOR
        if (contx == null)
        {
            Debug.LogError(log);
        }
        else Debug.LogError(log, contx);
#endif
    }
    public static void LogWarning(object log, UnityEngine.Object contx = null)
    {
#if EnableLogs || UNITY_EDITOR
        if (contx == null)
        {
            Debug.LogWarning(log);
        }
        else Debug.LogWarning(log, contx);
#endif
    }
}
