using CogniCache.Domain.Models;
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
            return new GetAllNotesQueryResponse(notes.Select(note => new NoteModel
            {
                Id = note.Id,
                Title = note.Title,
                Body = note.Body,
                Html = string.Empty,
                Tags = note.Tags,
                LastUpdatedDate = note.LastUpdatedDate
            }));
        }
    }

    public record GetAllNotesQueryResponse(IEnumerable<NoteModel> Notes);
}
