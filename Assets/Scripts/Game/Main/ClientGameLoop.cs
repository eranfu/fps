using Utils;

namespace Game.Main
{
    public class ClientGameLoop : GameRoot.IGameLoop
    {
        private enum ClientState
        {
            Browsing,
            Connecting
        }

        private StateMachine<ClientState> _stateMachine;
        private string _targetServer = "";
        private Networking.NetworkClient _networkClient;
        private int _connectRetryCount = 0;

        public bool Init(string[] args)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void LateUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void CmdConnect(string[] args)
        {
            ClientState currentState = _stateMachine.CurrentState();
            if (currentState == ClientState.Browsing)
            {
                _targetServer = args.Length > 0 ? args[0] : "127.0.0.1";
                _stateMachine.SwitchTo(ClientState.Connecting);
            }
            else if (currentState == ClientState.Connecting)
            {
                _networkClient.Disconnect();
                _targetServer = args.Length > 0 ? args[0] : "127.0.0.1";
                _connectRetryCount = 0;
            }
            else
            {
                GameDebug.Log($"Unable to connect from this state: {currentState}");
            }
        }
    }
}