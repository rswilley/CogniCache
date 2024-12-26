using CogniCache.Domain.Repositories.NoteRepository;

namespace CogniCache.Domain.Repositories.TagRepository
{
    public interface ITagRepository
    {
        int Create(Tag tag);
        List<Tag> GetAll();
    }
}
