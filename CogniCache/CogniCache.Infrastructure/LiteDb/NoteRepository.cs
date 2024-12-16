﻿using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Infrastructure.Services;
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

        public List<Note> GetAll()
        {
            using var db = new LiteDatabase(_config.DatabaseFilePath);

            var col = db.GetCollection<Note>("notes");
            return col.FindAll().ToList();
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
    }
}