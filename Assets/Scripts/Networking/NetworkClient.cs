using System.Collections.Generic;
using Utils;

namespace Networking
{
    public class NetworkClient
    {
        #region Inner define

        private enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        private class Counters : NetworkConnectionCounters
        {
        }

        private class ClientPackageInfo : PackageInfo
        {
        }

        private class ClientConnection : NetworkConnection<Counters, ClientPackageInfo>
        {
            private class EntityInfo
            {
            }

            public ConnectionState connectionState = ConnectionState.Connecting;
            private int _serverTime;
            private readonly List<EntityInfo> _entities = new List<EntityInfo>();
            private readonly List<int> _spawns = new List<int>();
            private readonly List<int> _despawns = new List<int>();
            private readonly List<int> _updates = new List<int>();

            public override void Reset()
            {
                base.Reset();
                _serverTime = 0;
                _entities.Clear();
                _spawns.Clear();
                _despawns.Clear();
                _updates.Clear();
            }
        }

        #endregion

        private ClientConnection _connection;
        private INetworkTransport _transport;

        public void Disconnect()
        {
            if (_connection == null)
                return;

            _transport.Disconnect(_connection.connectionId);

            // Note, we have to call OnDisconnect manually as disconnecting forcefully like this does not
            // generate an disconnect event from the transport layer
            OnDisconnect(_connection.connectionId);
        }

        private void OnDisconnect(int connectionId)
        {
            if (_connection != null && _connection.connectionId == connectionId)
            {
                if (_connection.connectionState == ConnectionState.Connected)
                {
                    GameDebug.Log("Disconnected from server");
                    GameDebug.Log(
                        $"Last package sent: {_connection.outSequence}. " +
                        $"Last package received: {_connection.inSequence} {NetworkUtils.StopWatch.ElapsedMilliseconds - _connection.inSequenceTime} ms ago");
                }
                else if (_connection.connectionState == ConnectionState.Connecting)
                {
                    GameDebug.Log("Server never replied when trying to connect ... disconnecting");
                }

                _connection.Reset();
                _connection = null;
            }
        }
    }
}