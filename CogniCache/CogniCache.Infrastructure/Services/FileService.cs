using CogniCache.Domain.Services;

namespace CogniCache.Infrastructure.Services
{
    public class FileService : IFileService
    {
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string GetCleanFileName(string fileName)
        {
            return string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void SaveFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}
