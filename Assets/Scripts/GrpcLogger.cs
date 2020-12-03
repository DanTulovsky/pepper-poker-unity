using System;
using UnityEngine;
using ILogger = Grpc.Core.Logging.ILogger;

internal class GrpcLogger : ILogger {
    void ILogger.Debug(string message) {
        Debug.Log("DEBUG: " + message);
    }

    void ILogger.Debug(string format, params object[] formatArgs) {
        Debug.LogFormat("DEBUG: " + format, formatArgs);
    }

    void ILogger.Error(string message) {
        Debug.LogError("ERROR: " + message);
    }

    void ILogger.Error(string format, params object[] formatArgs) {
        Debug.LogErrorFormat("ERROR: " + format, formatArgs);
    }

    void ILogger.Error(Exception exception, string message) {
        Debug.LogException(exception);
        Debug.LogError("ERROR: " + message);
    }

    ILogger ILogger.ForType<T>() {
        return new GrpcLogger();
    }

    void ILogger.Info(string message) {
        Debug.Log("INFO: " + message);
    }

    void ILogger.Info(string format, params object[] formatArgs) {
        Debug.LogFormat("INFO: " + format, formatArgs);
    }

    void ILogger.Warning(string message) {
        Debug.LogWarningFormat("WARN: " + message);
    }

    void ILogger.Warning(string format, params object[] formatArgs) {
        Debug.LogWarningFormat("WARN: " + format, formatArgs);
    }

    void ILogger.Warning(Exception exception, string message) {
        Debug.LogException(exception);
        Debug.LogWarning("WARN: " + message);
    }
}