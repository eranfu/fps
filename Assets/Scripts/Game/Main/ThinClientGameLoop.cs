using System;
using Utils;

namespace Game.Main
{
    public class ThinClientGameLoop : GameRoot.IGameLoop
    {
        private string _targetServer = "";

        public bool Init(string[] args)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new NotImplementedException();
        }

        public void LateUpdate()
        {
            throw new NotImplementedException();
        }

        public void CmdConnect(string[] args)
        {
            _targetServer = args.Length > 0 ? args[0] : "127.0.0.1";
            GameDebug.Log($"Will connect to: {_targetServer}");
        }
    }
}