namespace Networking
{
    public interface IDeserializable
    {
        void Deserialize(IInputStream inputStream);
    }
}