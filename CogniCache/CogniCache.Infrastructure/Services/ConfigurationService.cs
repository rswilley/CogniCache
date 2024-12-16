using CogniCache.Domain.Services;

namespace CogniCache.Infrastructure.Services;

public interface IConfigurationService
{
    void LoadConfiguration();
    void SaveConfiguration(string key, string value);
    Dictionary<string, string> GetConfiguration();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _config;
    private readonly IFileService _fileService;

    private Dictionary<string, string> _settings = [];

    public ConfigurationService(
        IConfiguration config,
        IFileService fileService)
    {
        _config = config;
        _fileService = fileService;
    }

    public void LoadConfiguration()
    {
        _fileService.CreateDirectory(_config.NotesDirectory);
        //TODO
    }

    public void SaveConfiguration(string key, string value)
    {
        //TODO
    }

    public Dictionary<string, string> GetConfiguration()
    {
        LoadConfiguration();
        return _settings;
    }
}