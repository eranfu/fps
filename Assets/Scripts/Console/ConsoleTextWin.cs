#if UNITY_STANDALONE_WIN
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Console
{
    public class ConsoleTextWin : IConsoleUi
    {
        private readonly bool _restoreFocus;
        private readonly string _title;
        private IntPtr _foregroundWindow;
        private float _resetWindowTime;
        private TextWriter _previousOutput;
        private string _currentLine;


        public ConsoleTextWin(string title, bool restoreFocus)
        {
            _title = title;
            _restoreFocus = restoreFocus;
        }

        public void Init()
        {
            if (!AttachConsole(0xffffffff))
            {
                if (_restoreFocus)
                {
                    _foregroundWindow = GetForegroundWindow();
                    _resetWindowTime = Time.time + 1;
                }

                AllocConsole();
            }

            _previousOutput = System.Console.Out;
            SetConsoleTitle(_title);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.Clear();
            System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput()) {AutoFlush = true});
            _currentLine = "";
            DrawInputLine();
        }

        private void DrawInputLine()
        {
            System.Console.CursorLeft = 0;
            System.Console.CursorTop = System.Console.BufferHeight - 1;
            System.Console.BackgroundColor = System.ConsoleColor.Blue;
            System.Console.Write(
                $"{_currentLine}{new string(' ', System.Console.BufferWidth - _currentLine.Length - 1)}");
            System.Console.CursorLeft = _currentLine.Length;
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void OutputString(string message)
        {
            throw new NotImplementedException();
        }

        public bool IsOpen()
        {
            throw new NotImplementedException();
        }

        public void SetOpen(bool open)
        {
            throw new NotImplementedException();
        }

        public void ConsoleUpdate()
        {
            throw new NotImplementedException();
        }

        public void ConsoleLateUpdate()
        {
            throw new NotImplementedException();
        }

        [DllImport("Kernel32.dll")]
        private static extern bool AttachConsole(uint processId);

        [DllImport("Kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("Kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("Kernel32.dll")]
        private static extern bool SetConsoleTitle(string title);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
#endif