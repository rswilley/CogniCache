﻿using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;

namespace CogniCache.Application.Commands
{
    public record DeleteNoteCommand(int NoteId);

    public class DeleteNoteCommandHandler : IRequest<DeleteNoteCommand, DeleteNoteCommandResponse>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ISearchRepository _searchRepository;

        public DeleteNoteCommandHandler(
            INoteRepository noteRepository,
            ISearchRepository searchRepository)
        {
            _noteRepository = noteRepository;
            _searchRepository = searchRepository;
        }

        public DeleteNoteCommandResponse Handle(DeleteNoteCommand request)
        {
            _noteRepository.Delete(request.NoteId);
            _searchRepository.DeleteById(request.NoteId);

            var remainingNotes = _noteRepository.GetAll().OrderByDescending(n => n.LastUpdatedDate);

            return new DeleteNoteCommandResponse(remainingNotes);
        }
    }

    public record DeleteNoteCommandResponse(IEnumerable<Note> RemainingNotes);
}
