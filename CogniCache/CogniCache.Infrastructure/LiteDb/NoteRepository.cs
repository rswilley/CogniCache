using CogniCache.Domain.Enums;
using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Services;
using LiteDB;

namespace CogniCache.Infrastructure.LiteDb
{
    public class NoteRepository : INoteRepository
    {
        private readonly IConfiguration _config;

        public NoteRepository(IConfiguration config)
        {
            _config = config;
        }

        public int Create(Note note)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            var id = col.Insert(note);
            col.EnsureIndex(n => n.CreatedDate);

            return id;
        }

        public Note Upsert(Note note)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            if (note.Id == 0)
            {
                var id = col.Insert(note);
                col.EnsureIndex(n => n.CreatedDate);

                note.Id = id;
                return note;
            }
            else
            {
                var toUpdate = col.FindById(note.Id);
                toUpdate.Title = note.Title;
                toUpdate.Body = note.Body;
                toUpdate.FileName = note.FileName;
                toUpdate.LastUpdatedDate = note.LastUpdatedDate;
                toUpdate.Tags = note.Tags;
                toUpdate.IsStarred = note.IsStarred;

                col.Update(toUpdate);
                return toUpdate;
            }
        }

        public void Delete(int id)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            col.Delete(id);
        }

        public void SetFilename(int id, string fileName)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            var toUpdate = col.FindById(id);
            toUpdate.FileName = fileName;
            col.Update(toUpdate);
        }

        public IEnumerable<Note> GetAll()
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);
            var col = db.GetCollection<Note>("notes");
            return col.FindAll().ToList();
        }

        public List<Note> GetManyPaginated(DateTime dateBegin, DateTime dateEnd, int offset, int limit, NoteSortMode? sortMode, string? tagPath)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            var query = col.Query();

            HandleDateRange();
            HandleSort();

            if (!string.IsNullOrEmpty(tagPath)) {
                query.Where(q => q.Tags.Contains(tagPath));
            }

            var results = query
                .Offset(offset)
                .Limit(limit);

            return results.ToList();

            void HandleDateRange()
            {
                switch (sortMode)
                {
                    case NoteSortMode.Created_Desc:
                    case NoteSortMode.Created_Asc:
                        query.Where(x => x.CreatedDate >= dateBegin && x.CreatedDate <= dateEnd);
                        break;
                    default:
                        query.Where(x => x.LastUpdatedDate >= dateBegin && x.LastUpdatedDate <= dateEnd);
                        break;
                }
            }

            void HandleSort()
            {
                switch (sortMode)
                {
                    case NoteSortMode.LastModified_Desc:
                        query.OrderByDescending(x => x.LastUpdatedDate);
                        break;
                    case NoteSortMode.LastModified_Asc:
                        query.OrderBy(x => x.LastUpdatedDate);
                        break;
                    case NoteSortMode.Created_Desc:
                        query.OrderByDescending(x => x.CreatedDate);
                        break;
                    case NoteSortMode.Created_Asc:
                        query.OrderBy(x => x.CreatedDate);
                        break;
                    case NoteSortMode.Title_Desc:
                        query.OrderByDescending(x => x.Title);
                        break;
                    case NoteSortMode.Title_Asc:
                        query.OrderBy(x => x.Title);
                        break;
                }
            }
        }

        public Note GetById(int id)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            return col.FindById(id);
        }

        public List<Note> GetManyByTagName(string tag)
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            return col.Find(c => c.Tags.Contains(tag)).ToList();
        }

        public List<string> GetAllTags()
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            col.EnsureIndex(c => c.Tags);

            var results = col.Query()
                .Select(c => new { c.Tags })
                .ToList()
                .SelectMany(t => t.Tags)
                .OrderBy(t => t)
                .Distinct();

            return results.ToList();
        }

        public bool HasNotes()
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            return col.Count() > 0;
        }
    }
}
