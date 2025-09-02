namespace CogniCache.Domain.Repositories.SearchRepository
{
    public class NoteSearchItem : IDocument
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}
