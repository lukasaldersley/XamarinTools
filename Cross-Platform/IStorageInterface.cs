using System.Threading.Tasks;

namespace TimeTracker
{
    //this code is taken from https://github.com/lukasaldersley/XamarinTools
    public interface IStorageInterface
    {
        string TestPlatforms(string message);
        Task WriteBytesAsync(string path, byte[] content);
        Task WriteStringAsync(string path, string content);
        Task<string> ReadStringAsync(string path);
        Task<byte[]> ReadBytesAsync(string path);
        Task DeleteFile(string path);
        Task WriteBytesPickLocationAsync(byte[] content, string name);
        Task WriteStringPickLocationAsync(string content, string name);
        Task<byte[]> ReadBytesPickLocationAsync();
        Task<string> ReadStringPickLocationAsync();
        Task ShareFileBytes(byte[] content, string name);
        Task ShareFileString(string content, string name);
    }
}
