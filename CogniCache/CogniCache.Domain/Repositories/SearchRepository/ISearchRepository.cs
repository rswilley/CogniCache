using CogniCache.Domain.Repositories.NoteRepository;

namespace CogniCache.Domain.Repositories.SearchRepository
{
    public interface ISearchRepository
    {
        void Create(Note note);
        void Update(Note note);
        void DeleteById(int noteId);
        SearchResult Search(string query);
    }
}
