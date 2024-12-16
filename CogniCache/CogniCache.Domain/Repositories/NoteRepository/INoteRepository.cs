﻿namespace CogniCache.Domain.Repositories.NoteRepository
{
    public interface INoteRepository
    {
        int Create(Note note);
        Note Upsert(Note note);
        void Delete(int id);
        Note GetById(int id);
        List<Note> GetAll();
        List<Note> GetManyByTagName(string tag);
    }
}