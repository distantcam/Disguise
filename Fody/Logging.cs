using System;
using System.Diagnostics;

[assembly: Anotar.Custom.LogMinimalMessage]

public static class LoggerFactory
{
    public static Action<string> LogInfo { get; set; }
    public static Action<string> LogWarn { get; set; }
    public static Action<string> LogError { get; set; }

    public static Logger GetLogger<T>()
    {
        return new Logger();
    }
}

public class Logger
{
    public void Debug(string format, params object[] args)
    {
        if (IsDebugEnabled)
            System.Diagnostics.Debug.WriteLine(format, args);
    }

    public void Debug(Exception exception, string format, params object[] args)
    {
        if (IsDebugEnabled)
        {
            System.Diagnostics.Debug.WriteLine(format, args);
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }

    public bool IsDebugEnabled { get { return Debugger.IsAttached; } }

    public void Information(string format, params object[] args)
    {
        if (IsInformationEnabled)
            LoggerFactory.LogInfo(string.Format(format, args));
    }

    public void Information(Exception exception, string format, params object[] args)
    {
        if (IsInformationEnabled)
            LoggerFactory.LogInfo(string.Format(format, args) + Environment.NewLine + exception);
    }

    public bool IsInformationEnabled { get { return LoggerFactory.LogInfo != null; } }

    public void Warning(string format, params object[] args)
    {
        if (IsWarningEnabled)
            LoggerFactory.LogWarn(string.Format(format, args));
    }

    public void Warning(Exception exception, string format, params object[] args)
    {
        if (IsWarningEnabled)
            LoggerFactory.LogWarn(string.Format(format, args) + Environment.NewLine + exception);
    }

    public bool IsWarningEnabled { get { return LoggerFactory.LogWarn != null; } }

    public void Error(string format, params object[] args)
    {
        if (IsErrorEnabled)
            LoggerFactory.LogError(string.Format(format, args));
    }

    public void Error(Exception exception, string format, params object[] args)
    {
        if (IsErrorEnabled)
            LoggerFactory.LogError(string.Format(format, args) + Environment.NewLine + exception);
    }

    public bool IsErrorEnabled { get { return LoggerFactory.LogError != null; } }
}