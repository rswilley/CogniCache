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
        IEnumerable<MemoModel> GetManyPaginated(int offset, int limit, string? sortBy, string? tagPath);
        IEnumerable<MemoModel> GetManyByTag(string tag);
        MemoModel? GetById(int id);
        string RemoveTitle(string body);

        int SaveNote(MemoModel note);
        void DeleteNote(int noteId);
        void ReindexNotes();
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

        public IEnumerable<MemoModel> GetManyPaginated(int offset, int limit, string? sortBy, string? tagPath)
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

        public IEnumerable<MemoModel> GetManyByTag(string tag)
        {
            var decodedTag = WebUtility.HtmlDecode(tag);
            var notes = _noteRepository
                .GetManyByTagName(decodedTag)
                .OrderByDescending(n => n.LastUpdatedDate)
                .Select(ToDomainModel);

            return notes;
        }

        public MemoModel? GetById(int id)
        {
            var note = _noteRepository.GetById(id);
            if (note == null)
                return null;

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

        public int SaveNote(MemoModel note)
        {
            var html = !string.IsNullOrEmpty(note.Html)
                ? note.Html
                : GetHtml(note.Content);
            var title = GetTitle(html);

            if (note.Id != 0)
            {
                var fileName = GetFilename(title, note.Id);
                var upsertedNote = _noteRepository.Upsert(new Note
                {
                    Id = note.Id,
                    Title = title,
                    Body = note.Content,
                    Tags = note.Tags.Count != 0
                        ? note.Tags
                        : [],
                    FileName = fileName,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsStarred = note.IsStarred
                });

                _searchRepository.Update(upsertedNote);
                _fileService.SaveFile(Path.Combine(_configuration.NotesDirectory, fileName), upsertedNote.Body);

                return upsertedNote.Id;
            }
            else
            {
                var newNote = _noteRepository.Upsert(new Note
                {
                    Id = note.Id,
                    Title = title,
                    Body = note.Content,
                    Tags = note.Tags.Count != 0
                        ? note.Tags
                        : [],
                    FileName = string.Empty,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsStarred = note.IsStarred
                });

                var fileName = GetFilename(title, newNote.Id);
                _noteRepository.SetFilename(newNote.Id, fileName);
                _searchRepository.Update(newNote);
                _fileService.SaveFile(Path.Combine(_configuration.NotesDirectory, fileName), newNote.Body);

                return newNote.Id;
            }
        }

        private string GetFilename(string title, int noteId)
        {
            var fileName = _fileService.GetCleanFileName($"{title}-{noteId}.md");
            return fileName;
        }

        public void DeleteNote(int noteId)
        {
            _noteRepository.Delete(noteId);
            _searchRepository.DeleteById(noteId);
        }

        public void ReindexNotes()
        {
            var allNotes = _noteRepository.GetAll().ToList();
            foreach (var note in allNotes)
            {
                _searchRepository.Update(note);
            }
        }

        private MemoModel ToDomainModel(Note note)
        {
            var html = GetHtml(note.Body);
            var snippet = GetPreview(html);

            return new MemoModel
            {
                Id = note.Id,
                Title = note.Title,
                Snippet = snippet,
                Content = note.Body,
                Html = html,
                Tags = note.Tags,
                CreatedDate = note.CreatedDate,
                LastUpdatedDate = note.LastUpdatedDate,
                IsStarred = note.IsStarred
            };
        }

        private string GetPreview(string html)
        {
            html = RemoveTitle(html);
            return html;
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
                        html = html.Replace(oldValue, $"<a href='/memos/{resultId}'>{note.Title}</a>");
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

        private static string RemoveLinks(string html)
        {
            string pattern = @"(<a.*?>.*?</a>)";
            string result = Regex.Replace(html, pattern, string.Empty);

            return result;
        }
    }
}
