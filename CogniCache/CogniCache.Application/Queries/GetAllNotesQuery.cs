using CogniCache.Domain.Repositories.NoteRepository;

namespace CogniCache.Application.Queries
{
    public record GetAllNotesQuery;

    public class GetAllNotesQueryHandler : IRequest<GetAllNotesQuery, GetAllNotesQueryResponse>
    {
        private readonly INoteRepository _noteRepository;

        public GetAllNotesQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public GetAllNotesQueryResponse Handle(GetAllNotesQuery request)
        {
            var notes = _noteRepository.GetAll().OrderByDescending(n => n.CreatedDate);
            return new GetAllNotesQueryResponse(notes);
        }
    }

    public record GetAllNotesQueryResponse(IEnumerable<Note> Notes);
}
