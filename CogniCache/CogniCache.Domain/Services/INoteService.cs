using CogniCache.Domain.Models;
using CogniCache.Domain.Repositories.NoteRepository;
using System.Net;
using System.Text.RegularExpressions;

namespace CogniCache.Domain.Services
{
    public interface INoteService
    {
        IEnumerable<NoteModel> GetAll();
        IEnumerable<NoteModel> GetManyByTag(string tag);
        NoteModel GetById(int id);
        string RemoveTitle(string body);
    }

    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IRenderService _renderService;

        public NoteService(
            INoteRepository noteRepository, 
            IRenderService renderService)
        {
            _noteRepository = noteRepository;
            _renderService = renderService;
        }

        public IEnumerable<NoteModel> GetAll()
        {
            var notes = _noteRepository.GetAll().OrderByDescending(n => n.CreatedDate);
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
    }
}
