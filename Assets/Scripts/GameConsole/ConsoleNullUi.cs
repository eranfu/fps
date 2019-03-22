namespace GameConsole
{
    public class ConsoleNullUi : IConsoleUi
    {
        public void Init()
        {
        }

        public void Shutdown()
        {
        }

        public void OutputString(string message)
        {
        }

        public bool IsOpen()
        {
            return false;
        }

        public void SetOpen(bool open)
        {
        }

        public void ConsoleUpdate()
        {
        }

        public void ConsoleLateUpdate()
        {
        }
    }
}