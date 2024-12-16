namespace CogniCache.Domain.Services
{
    public interface IFileService
    {
        string GetCleanFileName(string fileName);
        bool Exists(string path);
        void CreateDirectory(string path);
        void SaveFile(string path, string contents);
    }
}
