namespace Networking
{
    public interface ISerializable
    {
        void Serialize(IOutputStream outputStream);
    }
}