using CogniCache.Domain;
using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Services;
using System.Text.RegularExpressions;

namespace CogniCache.Application.Queries;

public record GetNoteByIdQuery(int NoteId, EditorMode Mode);

public class GetNoteByIdQueryHandler : IRequest<GetNoteByIdQuery, GetNoteByIdQueryResponse>
{
    private readonly INoteRepository _noteRepository;
    private readonly IRenderService _renderService;

    public GetNoteByIdQueryHandler(
        INoteRepository noteRepository,
        IRenderService renderService)
    {
        _noteRepository = noteRepository;
        _renderService = renderService;
    }

    public GetNoteByIdQueryResponse Handle(GetNoteByIdQuery request)
    {
        var body = _noteRepository.GetById(request.NoteId).Body;
        var html = string.Empty;

        if (request.Mode == EditorMode.Preview)
        {
            html = _renderService.ToHtml(body);
            html = CreateInternalLinks(html);
        }

        return new GetNoteByIdQueryResponse(body, html);
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

public record GetNoteByIdQueryResponse(string Body, string Html);