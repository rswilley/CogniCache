namespace CogniCache.Domain.Repositories.NoteRepository
{

    public class Note : IDocument
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
        public required string FileName { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public required DateTime LastUpdatedDate { get; set; }
        public List<string> Tags { get; set; } = ["Untagged"];
        public bool IsStarred { get; set; }
    }
}
