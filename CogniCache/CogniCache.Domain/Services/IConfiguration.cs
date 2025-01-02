namespace CogniCache.Domain.Services
{
    public interface IConfiguration
    {
        string AppDirectory { get; }
        string NotesDirectory { get; }
        string DatabaseFilePath { get; }
    }
}
