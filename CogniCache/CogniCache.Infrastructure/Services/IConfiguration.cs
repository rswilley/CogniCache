using CogniCache.Domain.Services;

namespace CogniCache.Infrastructure.Services;

public class Configuration : IConfiguration
{
    public string AppDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CogniCache");
    public string NotesDirectory => Path.Combine(AppDirectory, "notes");
    public string MemosDirectory => Path.Combine(AppDirectory, "memos");
    public string DatabaseFilePath => Path.Combine(AppDirectory, "CogniCache.db");
}
