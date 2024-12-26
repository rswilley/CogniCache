using CogniCache.Domain.Repositories.TagRepository;
using CogniCache.Infrastructure.Services;
using LiteDB;

namespace CogniCache.Infrastructure.LiteDb
{
    public class TagRepository : ITagRepository
    {
        private readonly IConfiguration _config;

        public TagRepository(IConfiguration config)
        {
            _config = config;
        }

        public int Create(Tag tag)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Tag>("tags");
            var id = col.Insert(tag);

            return id;
        }

        public List<Tag> GetAll()
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Tag>("tags");
            return col.FindAll().ToList();
        }
    }
}
