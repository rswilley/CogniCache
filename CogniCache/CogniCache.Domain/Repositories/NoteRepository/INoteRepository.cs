using CogniCache.Domain.Enums;

namespace CogniCache.Domain.Repositories.NoteRepository
{
    public interface INoteRepository
    {
        int Create(Note note);
        Note Upsert(Note note);
        void Delete(int id);
        Note GetById(int id);
        List<Note> GetManyPaginated(int offset, int limit, NoteSortMode? sortMode, string? tagPath);
        List<Note> GetManyByTagName(string tag);
        List<string> GetAllTags();
        bool HasNotes();
    }
}
