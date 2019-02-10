namespace IO
{
    public interface IDeserializable
    {
        void Deserialize(IInputStream inputStream);
    }
}