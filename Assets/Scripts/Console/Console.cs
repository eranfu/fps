using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using Utils.DebugOverlay;
using Utils.Pool;

namespace Console
{
    public interface IConsoleUi
    {
        void Init();
        void Shutdown();
        void OutputString(string message);
        bool IsOpen();
        void SetOpen(bool open);
        void ConsoleUpdate();
        void ConsoleLateUpdate();
    }

    public class Console
    {
        public delegate void MethodDelegate(string[] args);

        [ConfigVar(name = "config.showlastline", defaultValue = "0",
            description = "Show last logged line briefly at top of screen")]
        private static ConfigVar _consoleShowLastLine;

        private static IConsoleUi _consoleUi;
        private static string _lastMessage = "";
        private static double _timeLastMessage;
        private static readonly Dictionary<string, ConsoleCommand> Commands = new Dictionary<string, ConsoleCommand>();
        private static readonly List<string> PendingCommands = new List<string>();

        public static void Init(IConsoleUi consoleUi)
        {
            Debug.Assert(consoleUi != null);

            _consoleUi = consoleUi;
            _consoleUi.Init();
            AddCommand("help", CmdHelp, "Show available commands");
            AddCommand("vars", CmdVars, "Show available variables");
            AddCommand("wait", CmdWait, "Wait for next frame or level");
            AddCommand("waitload", CmdWaitLoad, "Wait for level load");
            AddCommand("exec", CmdExec, "Executes commands from file");
            Write("Console ready");
        }

        public static void Shutdown()
        {
            _consoleUi.Shutdown();
        }

        private static void OutputString(string message)
        {
            _consoleUi?.OutputString(message);
        }

        public static void Write(string message)
        {
            if (_consoleShowLastLine?.IntValue > 0)
            {
                _lastMessage = message;
                _timeLastMessage = Game.Main.Game.frameTime;
            }

            OutputString(message);
        }

        public static void AddCommand(string cmd, MethodDelegate func, string description, int tag = 0)
        {
            cmd = cmd.ToLower();
            if (Commands.ContainsKey(cmd))
            {
                Write($"Cannot add command {cmd} twice");
                return;
            }

            Commands.Add(cmd, new ConsoleCommand(cmd, func, description, tag));
        }

        public static void RemoveCommand(string cmd)
        {
            Commands.Remove(cmd);
        }

        public static void RemoveCommandsWithTag(int tag)
        {
            var removeList = SimpleObjectPool.Pop<List<string>>();
            foreach (var command in Commands.Values)
            {
                if (command.tag == tag)
                {
                    removeList.Add(command.name);
                }
            }

            foreach (string name in removeList)
            {
                RemoveCommand(name);
            }

            SimpleObjectPool.Push(removeList);
        }

        public static void ProcessCommandLineArgument(string[] arguments)
        {
            OutputString($"ProcessCommandLineArguments: {string.Join(" ", arguments)}");
            var commands = SimpleObjectPool.Pop<List<string>>();
            foreach (string argument in arguments)
            {
                bool newCommandStarting = argument.StartsWith("+") || argument.StartsWith("-");

                // skip leading arguments before we have seen '+' or '-'
                if (commands.Count == 0 && !newCommandStarting)
                {
                    continue;
                }

                if (newCommandStarting)
                {
                    commands.Add(argument);
                }
                else
                {
                    string command = $"{commands[commands.Count - 1]} {argument}";
                    commands[commands.Count - 1] = command;
                }
            }

            foreach (string command in commands)
            {
                EnqueueCommandNoHistory(command.Substring(1));
            }
        }

        public static void EnqueueCommandNoHistory(string command)
        {
            Debug.Log($"cmd: {command}");
            PendingCommands.Add(command);
        }

        public static bool IsOpen()
        {
            return _consoleUi.IsOpen();
        }

        public static void SetOpen(bool open)
        {
            _consoleUi.SetOpen(open);
        }

        public static void ConsoleUpdate()
        {
            double lastMessageTime = Game.Main.Game.frameTime - _timeLastMessage;
            if (lastMessageTime < 1)
            {
                DebugOverlay.Write(0, 0, _lastMessage);
            }
        }

        private static void CmdExec(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void CmdWaitLoad(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void CmdWait(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void CmdVars(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void CmdHelp(string[] args)
        {
            throw new NotImplementedException();
        }

        private class ConsoleCommand
        {
            public string name;
            public MethodDelegate method;
            public string description;
            public int tag;

            public ConsoleCommand(string name, MethodDelegate method, string description, int tag)
            {
                this.name = name;
                this.method = method;
                this.description = description;
                this.tag = tag;
            }
        }
    }
}