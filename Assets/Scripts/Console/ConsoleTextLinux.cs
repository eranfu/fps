using System.Threading;

#if !UNITY_STANDALONE_LINUX

namespace Console
{
    public class ConsoleTextLinux : IConsoleUi
    {
        private Thread _readerThread;
        private string _currentLine;

        public void Init()
        {
            bool isDumb = IsDumb();
            System.Console.WriteLine($"Dumb console: {isDumb}");
            if (isDumb)
            {
                _readerThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    var buf = new char[1024];
                    while (true)
                    {
                        int count = System.Console.In.Read(buf, 0, buf.Length);
                        if (count > 0)
                        {
                            _currentLine = new string(buf, 0, count);
                        }
                        else
                        {
                            break;
                        }
                    }
                });
                _readerThread.Start();
            }

            System.Console.Clear();
            _currentLine = "";
            DrawInputLine();
        }

        private void DrawInputLine()
        {
            if (IsDumb())
                return;

            System.Console.CursorLeft = 0;
            System.Console.CursorTop = System.Console.BufferHeight - 1;
            System.Console.Write(
                $"{_currentLine}{new string(' ', System.Console.BufferWidth - _currentLine.Length - 1)}");
            System.Console.CursorLeft = _currentLine.Length;
        }

        private static bool IsDumb()
        {
            return System.Console.BufferWidth == 0 || System.Console.IsInputRedirected ||
                   System.Console.IsOutputRedirected;
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        public void OutputString(string message)
        {
            throw new System.NotImplementedException();
        }

        public bool IsOpen()
        {
            throw new System.NotImplementedException();
        }

        public void SetOpen(bool open)
        {
            throw new System.NotImplementedException();
        }

        public void ConsoleUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void ConsoleLateUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}

#endif