using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using CogniCache.Domain.Services;
using CogniCache.Infrastructure.Services;
using System.Text.RegularExpressions;

namespace CogniCache.Application.Commands
{
    public record SaveNoteCommand(int NoteId, string Body, string Html);

    public class SaveNoteCommandHandler : IRequest<SaveNoteCommand, SaveNoteCommandResponse>
    {
        private readonly IConfiguration _configuration;
        private readonly INoteRepository _noteRepository;
        private readonly ISearchRepository _searchRepository;
        private readonly IFileService _fileService;
        private readonly IIdService _idService;

        public SaveNoteCommandHandler(
            IConfiguration configuration,
            INoteRepository noteRepository,
            ISearchRepository searchRepository,
            IFileService fileService,
            IIdService idService)
        {
            _configuration = configuration;
            _noteRepository = noteRepository;
            _searchRepository = searchRepository;
            _fileService = fileService;
            _idService = idService;
        }

        public SaveNoteCommandResponse Handle(SaveNoteCommand request)
        {
            var title = GetTitle(request.Html);
            var tags = GetTags(request.Html);

            var fileNameSuffix = request.NoteId == 0
                ? _idService.Generate(DateTime.UtcNow.Ticks).ToString()
                : request.NoteId.ToString();
            var fileName = _fileService.GetCleanFileName($"{title}-{fileNameSuffix}.md");

            var upsertedNote = _noteRepository.Upsert(new Note
            {
                Id = request.NoteId,
                Title = title,
                Body = request.Body,
                Tags = tags,
                FileName = fileName,
                LastUpdatedDate = DateTime.UtcNow
            });
            _searchRepository.Update(upsertedNote);

            _fileService.SaveFile(Path.Combine(_configuration.NotesDirectory, fileName), request.Body);

            return new SaveNoteCommandResponse(upsertedNote.Id);
        }

        private static string GetTitle(string html)
        {
            string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";

            Match match = Regex.Match(html, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "Untitled";
        }

        private static List<string> GetTags(string html)
        {
            html = RemoveLinks(html);

            string pattern = @"#\w+";

            MatchCollection matches = Regex.Matches(html, pattern);

            var tags = new List<string>();
            foreach (Match match in matches)
            {
                if (tags.Contains(match.Value))
                {
                    continue;
                }
                tags.Add(match.Value);
            }
            return tags;
        }

        private static string RemoveLinks(string html)
        {
            string pattern = @"(<a.*?>.*?</a>)";
            string result = Regex.Replace(html, pattern, string.Empty);

            return result;
        }
    }

    public record SaveNoteCommandResponse(int SavedNoteId);
}
