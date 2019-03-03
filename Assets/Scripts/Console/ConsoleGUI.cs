using UnityEngine;
using UnityEngine.UI;

namespace Console
{
    public class ConsoleGUI : MonoBehaviour, IConsoleUi
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Text buildIdText;

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

        public void Init()
        {
            buildIdText.text = $"{Game.Main.Game.game.BuildId} ({Application.unityVersion})";
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