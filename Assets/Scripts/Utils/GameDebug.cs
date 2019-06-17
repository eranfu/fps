using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;
using Console = GameConsole.Console;

namespace Utils
{
    //
    // Logging of messages
    //
    // There are three different types of messages:
    //
    // Debug.Log/Warn/Error coming from unity (or code, e.g. packages, not using GameDebug)
    //    These get caught here and sent onto the console and into our log file
    // GameDebug.Log/Warn/Error coming from game
    //    These gets sent onto the console and into our log file
    //    *IF* we are in editor, they are also sent to Debug.* so they show up in editor Console window
    // Console.Write
    //    Only used for things that should not be logged. Typically responses to user commands. Only shown on Console.
    //

    public static class GameDebug
    {
        private static Thread _writeFileThread;
        private static ConcurrentQueue<(string message, string stacktrace, LogType type)> _logQueue;
        private static AutoResetEvent _logEvent;

        public static void Init(string logFilePath, string logBaseName)
        {
            Exception lastException = null;
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    string filePath = $"{logFilePath}/{logBaseName}{(i == 0 ? "" : $"_{i}")}.log";
                    StreamWriter logFile = File.CreateText(filePath);
                    logFile.AutoFlush = true;
                    _logQueue = new ConcurrentQueue<(string message, string stacktrace, LogType type)>();
                    _logEvent = new AutoResetEvent(false);
                    _writeFileThread = new Thread(() =>
                    {
                        while (true)
                        {
                            if (_logQueue.TryDequeue(out (string message, string stacktrace, LogType type) log))
                            {
                                if (log.message == null)
                                {
                                    logFile.Close();
                                    break;
                                }

                                switch (log.type)
                                {
                                    case LogType.Error:
                                        logFile.WriteLine("[Error] {0}\nstack:\n{1}", log.message, log.stacktrace);
                                        break;
                                    case LogType.Assert:
                                        logFile.WriteLine("[Assert Failed] {0}\nstack:\n{1}", log.message,
                                            log.stacktrace);
                                        break;
                                    case LogType.Warning:
                                        logFile.WriteLine("[Warning] {0}\nstack:\n{1}", log.message, log.stacktrace);
                                        break;
                                    case LogType.Log:
                                        logFile.WriteLine("[Log] {0}\nstack:\n{1}", log.message, log.stacktrace);
                                        break;
                                    case LogType.Exception:
                                        logFile.WriteLine("[Exception] {0}\nstack:\n{1}", log.message, log.stacktrace);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            else
                            {
                                _logEvent.WaitOne();
                            }
                        }
                    });
                    _writeFileThread.Start();

                    Application.logMessageReceived += Write;
                    Log($"GameDebug initialized. Logging to {filePath}");
                    return;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            if (lastException != null)
                throw lastException;
        }

        public static void Shutdown()
        {
            Application.logMessageReceived -= Write;
            Write(null, null, LogType.Log);
            _writeFileThread.Join();
        }

        private static void Write(string message, string stacktrace, LogType type)
        {
            _logQueue.Enqueue((message, stacktrace, type));
            _logEvent.Set();
            Console.WriteLog(message, stacktrace, type);
        }

        public static void Log(string message)
        {
            Debug.LogFormat("[frame: {0}] {1}", Time.frameCount, message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarningFormat("[frame: {0}] {1}", Time.frameCount, message);
        }

        public static void LogError(string message)
        {
            Debug.LogErrorFormat("[frame: {0}] {1}", Time.frameCount, message);
        }
    }
}