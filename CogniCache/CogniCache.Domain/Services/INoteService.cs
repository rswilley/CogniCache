using CogniCache.Domain.Enums;
using CogniCache.Domain.Models;
using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using System.Net;
using System.Text.RegularExpressions;

namespace CogniCache.Domain.Services
{
    public interface INoteService
    {
        bool HasNotes();
        IEnumerable<NoteModel> GetManyPaginated(int offset, int limit, string? sortBy, string? tagPath);
        IEnumerable<NoteModel> GetManyByTag(string tag);
        NoteModel GetById(int id);
        string RemoveTitle(string body);

        int SaveNote(NoteModel note);
        void DeleteNote(int noteId);
    }

    public class NoteService : INoteService
    {
        private readonly IConfiguration _configuration;
        private readonly INoteRepository _noteRepository;
        private readonly ISearchRepository _searchRepository;
        private readonly IRenderService _renderService;
        private readonly IFileService _fileService;
        private readonly IIdService _idService;

        public NoteService(
            IConfiguration configuration,
            INoteRepository noteRepository,
            ISearchRepository searchRepository,
            IRenderService renderService,
            IFileService fileService,
            IIdService idService)
        {
            _configuration = configuration;
            _noteRepository = noteRepository;
            _searchRepository = searchRepository;
            _renderService = renderService;
            _fileService = fileService;
            _idService = idService;
        }

        public bool HasNotes()
        {
            return _noteRepository.HasNotes();
        }

        public IEnumerable<NoteModel> GetManyPaginated(int offset, int limit, string? sortBy, string? tagPath)
        {
            bool desc = false;
            NoteSortMode? sortMode = null;
            if (!string.IsNullOrEmpty(sortBy))
            {
                desc = sortBy[..1] == "-";
                var asc = sortBy[..1] == "+";
                if (!desc && !asc)
                {
                    sortBy = "+" + sortBy;
                    asc = true;
                }

                var direction = desc ? "_Desc": "_Asc";
                var sortByEnumValue = string.Concat(sortBy.AsSpan(1), direction);
                sortMode = Enum.Parse<NoteSortMode>(sortByEnumValue);
            } else
            {
                sortMode = NoteSortMode.LastModified_Desc;
            }

            var notes = _noteRepository.GetManyPaginated(offset, limit, sortMode, tagPath);
            return notes.Select(ToDomainModel);
        }

        public IEnumerable<NoteModel> GetManyByTag(string tag)
        {
            var decodedTag = WebUtility.HtmlDecode(tag);
            var notes = _noteRepository
                .GetManyByTagName(tag)
                .OrderByDescending(n => n.LastUpdatedDate)
                .Select(ToDomainModel);

            return notes;
        }

        public NoteModel GetById(int id)
        {
            var note = _noteRepository.GetById(id);
            
            return ToDomainModel(note);
        }

        public string RemoveTitle(string body)
        {
            string pattern = @"<h1\b[^>]*>(.*?)<\/h1>";
            string title = "";

            Match match = Regex.Match(body, pattern);
            if (match.Success)
            {
                title = match.Groups[1].Value;
            }

            if (string.IsNullOrEmpty(title))
                return body;

            return body.Replace(title, "");
        }

        public int SaveNote(NoteModel note)
        {
            var title = GetTitle(note.Html);
            var tags = note.Tags.Any() ? note.Tags : GetTags(note.Html);

            var fileNameSuffix = note.Id == 0
                ? _idService.Generate(DateTime.UtcNow.Ticks).ToString()
                : note.Id.ToString();
            var fileName = _fileService.GetCleanFileName($"{title}-{fileNameSuffix}.md");

            var upsertedNote = _noteRepository.Upsert(new Note
            {
                Id = note.Id,
                Title = title,
                Body = note.Body,
                Tags = tags,
                FileName = fileName,
                LastUpdatedDate = DateTime.UtcNow,
                IsStarred = note.IsStarred
            });

            _searchRepository.Update(upsertedNote);
            _fileService.SaveFile(Path.Combine(_configuration.NotesDirectory, fileName), note.Body);

            return upsertedNote.Id;
        }

        public void DeleteNote(int noteId)
        {
            _noteRepository.Delete(noteId);
            _searchRepository.DeleteById(noteId);
        }

        private NoteModel ToDomainModel(Note note)
        {
            var html = GetHtml(note.Body);
            var snippet = GetSnippet(html);

            return new NoteModel
            {
                Id = note.Id,
                Title = note.Title,
                Snippet = snippet,
                Body = note.Body,
                Html = html,
                Tags = note.Tags,
                CreatedDate = note.CreatedDate ?? DateTime.MinValue,
                LastUpdatedDate = note.LastUpdatedDate,
                IsStarred = note.IsStarred
            };
        }

        private string GetSnippet(string html)
        {
            html = RemoveTitle(html);
            var htmlStripped = Regex.Replace(html, "<.*?>", string.Empty);
            if (htmlStripped.Length >= 45)
            {
                return string.Concat(htmlStripped.AsSpan(0, 45), "...");
            } else
            {
                return htmlStripped;
            }
        }

        private string GetHtml(string body)
        {
            var html = _renderService.ToHtml(body);
            html = CreateInternalLinks(html);

            return html;
        }

        private string CreateInternalLinks(string html)
        {
            string pattern = @"{@note:(\d+)}";

            MatchCollection matches = Regex.Matches(html, pattern);

            foreach (Match match in matches)
            {
                var noteId = match.Groups[1].Value;
                if (int.TryParse(noteId, out int resultId))
                {
                    var oldValue = "{@note:" + resultId + "}";
                    var note = _noteRepository.GetById(resultId);

                    if (note is null)
                    {
                        html = html.Replace(oldValue, "<a href='#'>Invalid Link</a>");
                    }
                    else
                    {
                        html = html.Replace(oldValue, $"<a href='/notes/edit/{resultId}/1'>{note.Title}</a>");
                    }
                }
            }

            return html;
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
}
