using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole
{
    public class ConsoleGUI : MonoBehaviour, IConsoleUi
    {
        private readonly List<string> _lines = new List<string>();
        [SerializeField] private Text buildIdText;
        [SerializeField] private InputField inputField;
        [SerializeField] private Text textArea;

        public void Init()
        {
            buildIdText.text = $"{Game.Main.Game.game.BuildId} ({Application.unityVersion})";
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void OutputString(string message)
        {
            _lines.Add(message);
            int count = Mathf.Min(100, _lines.Count);
            int start = _lines.Count - count;
            textArea.text = string.Join("\n", _lines.GetRange(start, count));
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

        private void Awake()
        {
            inputField.onEndEdit.AddListener(OnSubmit);
        }

        private void OnSubmit(string value)
        {
            if (!Input.GetKey(KeyCode.KeypadEnter) && !Input.GetKey(KeyCode.Return))
                return;

            inputField.text = "";
            inputField.ActivateInputField();

            Console.EnqueueCommand(value);
        }
    }
}