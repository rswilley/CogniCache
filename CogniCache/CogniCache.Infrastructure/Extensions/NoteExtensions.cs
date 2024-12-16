using CogniCache.Domain.Repositories.NoteRepository;
using CogniCache.Domain.Repositories.SearchRepository;
using Lucene.Net.Documents;

namespace CogniCache.Infrastructure.Extensions
{
    internal static class NoteExtensions
    {
        public static Document ToDocument(this Note note)
        {
            var doc = new Document
            {
                new StringField(nameof(Note.Id), note.Id.ToString(), Field.Store.YES),
                new TextField(nameof(Note.Title), note.Title, Field.Store.YES),
                new TextField(nameof(Note.Body), note.Body, Field.Store.YES)
            };

            foreach (var tag in note.Tags)
            {
                doc.Fields.Add(new StringField(nameof(Note.Tags), tag, Field.Store.YES));
            }

            return doc;
        }

        public static NoteSearchItem ToNoteSearchItem(this Document document)
        {
            var tags = new List<string>();
            foreach (var field in document.Fields.Where(f => f.Name == nameof(Note.Tags)))
            {
                tags.Add(field.GetStringValue());
            }

            return new NoteSearchItem
            {
                Id = Convert.ToInt32(document.Get(nameof(Note.Id))),
                Title = document.Get(nameof(Note.Title)),
                Tags = tags,
                Body = document.Get(nameof(Note.Body))
            };
        }
    }
}
