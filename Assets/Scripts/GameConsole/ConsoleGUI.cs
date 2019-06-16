using System;
using System.Collections.Generic;
using Game.Main;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole
{
    public class ConsoleGUI : MonoBehaviour, IConsoleUi
    {
        private readonly List<string> _lines = new List<string>();
        [SerializeField] private Transform panel;
        [SerializeField] private Text buildIdText;
        [SerializeField] private InputField inputField;
        [SerializeField] private Text textArea;

        public void Init()
        {
            buildIdText.text = $"{GameRoot.gameRoot.BuildId} ({Application.unityVersion})";
        }

        public void Shutdown()
        {
        }

        public void OutputString(string message)
        {
            _lines.Add(message);
            int count = Mathf.Min(100, _lines.Count);
            int start = _lines.Count - count;
            textArea.text = string.Join("\n", _lines.GetRange(start, count).ToArray());
        }

        public bool IsOpen()
        {
            throw new NotImplementedException();
        }

        public void SetOpen(bool open)
        {
            GameRoot.Input.SetBlock(GameRoot.Input.Blocker.Console, open);
            panel.gameObject.SetActive(open);
            if (open)
            {
                inputField.ActivateInputField();
            }
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