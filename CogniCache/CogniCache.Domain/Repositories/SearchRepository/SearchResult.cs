namespace CogniCache.Domain.Repositories.SearchRepository
{
    public class SearchResult
    {
        public int TotalCount { get; init; }
        public IEnumerable<SearchSnippet>? Snippets { get; init; }
    }

    public class SearchSnippet
    {
        public int NoteId { get; init; }
        public required string Title { get; init; }
        public required string Snippet { get; init; }
    }
}
