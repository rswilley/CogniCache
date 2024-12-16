namespace CogniCache.Infrastructure.Services;

public interface IConfiguration
{
    string AppDirectory { get; }
    string NotesDirectory { get; }
    string DatabaseFilePath { get; }
}

public class Configuration : IConfiguration
{
    public string AppDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CogniCache");
    public string NotesDirectory => Path.Combine(AppDirectory, "notes");
    public string DatabaseFilePath => Path.Combine(AppDirectory, "CogniCache.db");
}
