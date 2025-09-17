using CogniCache.Domain.Enums;

namespace CogniCache.Domain.Repositories.NoteRepository
{
    public interface INoteRepository
    {
        int Create(Note note);
        Note Upsert(Note note);
        void Delete(int id);
        void SetFilename(int id, string fileName);
        Note GetById(int id);
        IEnumerable<Note> GetAll();
        List<Note> GetManyPaginated(DateTime? dateBegin, DateTime? dateEnd, int offset, int limit, NoteSortMode? sortMode, string? tagPath);
        List<Note> GetManyByTagName(string tag);
        List<string> GetAllTags();
        bool HasNotes();
    }
}
