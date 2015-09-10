namespace Reyna.Interfaces
{
    public interface IEncryptionChecker
    {
        bool DbEncrypted();

        void EncryptDb(byte[] password);
    }
}