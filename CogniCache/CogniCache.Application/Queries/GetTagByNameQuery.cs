using CogniCache.Domain.Repositories.NoteRepository;

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
            var notes = _noteRepository.GetManyByTagName(request.Tag).OrderByDescending(n => n.LastUpdatedDate);

            return new GetTagByNameQueryResponse(notes);
        }
    }

    public record GetTagByNameQueryResponse(IEnumerable<Note> Notes);
}
