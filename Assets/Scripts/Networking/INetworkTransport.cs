namespace Networking
{
    public interface INetworkTransport
    {
        void Disconnect(int connectionId);
    }
}