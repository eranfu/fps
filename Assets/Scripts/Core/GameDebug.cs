using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Core
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
    //    Only used for things that should not be logged. Typically reponses to user commands. Only shown on Console.
    //

    public static class GameDebug
    {
        private static Thread _writeFileThread;
        private static ConcurrentQueue<string> _logQueue;
        private static AutoResetEvent _logEvent;

        public static void Init(string logFilePath, string logBaseName)
        {
            Application.logMessageReceived += LogCallback;

            var filePath = "<none>";
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    filePath = $"{logFilePath}/{logBaseName}{(i == 0 ? "" : $"_{i}")}.log";
                    StreamWriter logFile = File.CreateText(filePath);
                    logFile.AutoFlush = true;
                    _logQueue = new ConcurrentQueue<string>();
                    _logEvent = new AutoResetEvent(false);
                    _writeFileThread = new Thread(() =>
                    {
                        while (true)
                        {
                            if (_logQueue.TryDequeue(out string log))
                            {
                                if (log == null)
                                {
                                    logFile.WriteLine("GameDebug shutdown");
                                    logFile.Close();
                                    break;
                                }

                                logFile.WriteLine(log);
                            }
                            else
                            {
                                _logEvent.WaitOne();
                            }
                        }
                    });
                    _writeFileThread.Start();
                    break;
                }
                catch
                {
                    filePath = "<none>";
                }
            }

            Debug.Log($"GameDebug initialized. Logging to {filePath}");
        }

        public static void Shutdown()
        {
            Application.logMessageReceived -= LogCallback;
            Write(null);
            _writeFileThread.Join();
        }

        private static void Write(string log)
        {
            _logQueue.Enqueue(log);
            _logEvent.Set();
        }

        private static void LogCallback(string message, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    Write($"{Time.frameCount}: [Error] {message}\nstack:\n{stacktrace}");
                    break;
                case LogType.Assert:
                    Write($"{Time.frameCount}: [Assert Failed] {message}\nstack:\n{stacktrace}");
                    break;
                case LogType.Warning:
                    Write($"{Time.frameCount}: [Warning] {message}\nstack:\n{stacktrace}");
                    break;
                case LogType.Log:
                    Write($"{Time.frameCount}: [Log] {message}\nstack:\n{stacktrace}");
                    break;
                case LogType.Exception:
                    Write($"{Time.frameCount}: [Exception] {message}\nstack:\n{stacktrace}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}