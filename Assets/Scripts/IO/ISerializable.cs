namespace IO
{
    public interface ISerializable
    {
        void Serialize(IOutputStream outputStream);
    }
}