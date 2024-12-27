using CogniCache.Domain.Models;
using CogniCache.Domain.Repositories.NoteRepository;
using System.Net;

namespace CogniCache.Application.Queries
{
    public record GetTagByNameQuery(string Tag);

    public class GetTagByIdQueryHandler : IRequest<GetTagByNameQuery, GetTagByNameQueryResponse>
    {
        private readonly INoteRepository _noteRepository;

        public GetTagByIdQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public GetTagByNameQueryResponse Handle(GetTagByNameQuery request)
        {
            var tag = WebUtility.HtmlDecode(request.Tag);
            var notes = _noteRepository
                .GetManyByTagName(tag)
                .OrderByDescending(n => n.LastUpdatedDate)
                .Select(note => new NoteModel {
                    Id = note.Id,
                    Title = note.Title,
                    Body = note.Body,
                    Html = string.Empty,
                    Tags = note.Tags,
                    LastUpdatedDate = note.LastUpdatedDate
                }
            );

            return new GetTagByNameQueryResponse(notes);
        }
    }

    public record GetTagByNameQueryResponse(IEnumerable<NoteModel> Notes);
}
